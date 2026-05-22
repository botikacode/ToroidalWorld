using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.Graphics;
using System;
using System.Collections.Generic;
using ToroidalWorld.GameEngine;
using ToroidalWorld.GameLogic.Components;
using ToroidalWorld.GameLogic.Entities.Components;
using ToroidalWorld.GameLogic.Entities.Definitions;
using ToroidalWorld.GameLogic.Physics;
using ToroidalWorld.GameLogic.Physics.Components;

namespace ToroidalWorld.GameLogic.Entities
{
    public static class EntityFactory
    {
        private const string DefaultPlayerBoatName = "Boat-001";
        private const string DefaultTurretName = "Turret-001";
        private const string DefaultEnemyBaseName = "EnemyBase-001";

        private const string DefaultExperienceSprite = "experience_orb_small";

        public static Entity CreatePlayer(World world, Vector2 position, Vector2 scale)
        {
            return CreatePlayer(world, boatName: DefaultPlayerBoatName, position, scale);
        }

        public static Entity CreatePlayer(World world, string boatName, Vector2 position, Vector2 scale)
        {
            BoatDefinition def = ResourceManager.GetBoatDefinition(boatName);

            string spriteKey = string.IsNullOrWhiteSpace(def.SpriteSheet) ? def.Name : def.SpriteSheet;

            var transform = new Transform2(position: position, scale: scale);

            var animPlayer = new AnimatedSprite(ResourceManager.GetSpriteSheet(spriteKey), "Idle");
            animPlayer.Origin = new Vector2(animPlayer.Size.X / 2f, animPlayer.Size.Y / 2f);

            var entity = world.CreateEntity();
            entity.Attach(transform);
            entity.Attach(animPlayer);

            entity.Attach(new EntityFlagsComponent(EntityFlags.Player));

            float baseMaxSpeed = def.MaxSpeed;
            float baseAcceleration = def.Acceleration;
            float baseRotationSpeed = def.RotationSpeed;

            entity.Attach(new PlayerLevelComponent { Level = 0 });

            entity.Attach(new StatMultipliersComponent
            {
                DamageMultiplier = 1f,
                AreaDamageMultiplier = 1f,
                AreaRadiusMultiplier = 1f,
                MoveSpeedMultiplier = 1f
            });

            entity.Attach(new BaseStatsComponent(baseMaxSpeed, baseAcceleration, baseRotationSpeed));

            entity.Attach(new MoveStatsComponent { MaxSpeed = baseMaxSpeed, Acceleration = baseAcceleration, RotationSpeed = baseRotationSpeed });
            entity.Attach(new BoatMoveComponent());
            entity.Attach(new MovementIntent());
            entity.Attach(new MovementState(transform));

            if (def.Icebreaker != null && def.Icebreaker.Enabled)
            {
                var c = new IcebreakerComponent
                {
                    CooldownSeconds = def.Icebreaker.CooldownSeconds,
                    DeltaPerHit = def.Icebreaker.DeltaPerHit,
                    ForwardTiles = def.Icebreaker.ForwardTiles,
                    HalfWidthTiles = def.Icebreaker.HalfWidthTiles,
                    BreakingLoopSfxKey = def.Icebreaker.BreakingLoopSfxKey
                };

                if (def.Icebreaker.Probe != null)
                {
                    c.ProbeEnabled = def.Icebreaker.Probe.Enabled;
                    c.ProbeForwardTiles = def.Icebreaker.Probe.ForwardTiles;
                    c.ProbeHalfWidthTiles = def.Icebreaker.Probe.HalfWidthTiles;
                    c.ProbeCooldownSeconds = def.Icebreaker.Probe.CooldownSeconds;
                    c.ProbeDeltaPerHit = def.Icebreaker.Probe.DeltaPerHit;
                    c.SlowdownPerSolidTile = def.Icebreaker.Probe.SlowdownPerSolidTile;
                    c.MinSpeedMultiplier = def.Icebreaker.Probe.MinSpeedMultiplier;
                }

                entity.Attach(c);
            }

            int colliderWidth = def.ColliderWidth;
            int colliderHeight = def.ColliderHeight;

            entity.Attach(CreateWakeTrailForCollider(colliderWidth, colliderHeight));

            entity.Attach(new TerrainCollider(width: colliderWidth, height: colliderHeight, rotates: true));
            entity.Attach(new EntityCollider(
                width: colliderWidth,
                height: colliderHeight,
                layer: CollisionLayer.Player,
                collidesWith: CollisionLayer.Enemy | CollisionLayer.EnemyProjectile,
                isSolid: true,
                rotates: true));

            entity.Attach(new DamageTakenCooldownComponent { CooldownSeconds = 0.25f });
            entity.Attach(new HealthComponent(maxHealth: def.Health));
            entity.Attach(new HealthRegenComponent());

            var turretMounts = new TurretMountsComponent();
            entity.Attach(turretMounts);

            CreateTurretsFromMountDefinitions(world, entity.Id, def.TurretMounts, turretMounts);

            return entity;
        }

        private static WaterWakeTrailComponent CreateWakeTrailForCollider(int colliderWidth, int colliderHeight)
        {
            const float refWidth = 20f;
            const float refHeight = 42f;

            float w = colliderWidth <= 0 ? refWidth : colliderWidth;
            float h = colliderHeight <= 0 ? refHeight : colliderHeight;

            float wScale = w / refWidth;
            float hScale = h / refHeight;

            if (wScale < 0.25f) wScale = 0.25f;
            if (hScale < 0.25f) hScale = 0.25f;

            var wake = new WaterWakeTrailComponent();

            wake.StartScale = new Vector2(0.2f * hScale, 0.8f * wScale);
            wake.EndScale = new Vector2(1.4f * hScale, 0.55f * wScale);

            wake.BehindOffsetPixels = 10f * hScale;

            wake.MinDistancePixels = 6f * wScale;

            return wake;
        }

        private static void CreateTurretsFromMountDefinitions(World world, int parentEntityId, List<TurretMountDefinition> mounts, TurretMountsComponent turretMounts)
        {
            if (mounts == null || mounts.Count == 0)
                return;

            foreach (var mount in mounts)
            {
                if (mount == null)
                    continue;

                string turretName = string.IsNullOrWhiteSpace(mount.Turret) ? DefaultTurretName : mount.Turret;

                var localOffset = new Vector2(mount.LocalOffsetX, mount.LocalOffsetY);
                float localRotation = mount.LocalRotationRadians;

                var turret = CreateTurret(world, turretName, parentEntityId, localOffset: localOffset, localRotation: localRotation);

                turretMounts?.Mounts.Add(new TurretMountState(
                    localOffset: localOffset,
                    localRotationRadians: localRotation,
                    turretEntityId: turret.Id,
                    turretName: turretName));
            }
        }

        public static Entity CreateTurret(World world, int parentEntityId)
        {
            return CreateTurret(world, turretName: DefaultTurretName, parentEntityId: parentEntityId, localOffset: Vector2.Zero);
        }

        public static Entity CreateTurret(World world, string turretName, int parentEntityId)
        {
            return CreateTurret(world, turretName, parentEntityId, localOffset: Vector2.Zero);
        }

        public static Entity CreateTurret(World world, string turretName, int parentEntityId, Vector2 localOffset, float localRotation = 0f)
        {
            TurretDefinition def = ResourceManager.GetTurretDefinition(turretName);

            string spriteKey = string.IsNullOrWhiteSpace(def.SpriteSheet) ? def.Name : def.SpriteSheet;

            var turretSprite = new AnimatedSprite(ResourceManager.GetSpriteSheet(spriteKey), "Idle");
            turretSprite.Origin = new Vector2(turretSprite.Size.X / 2f, turretSprite.Size.Y / 2f);

            string projectileName = string.IsNullOrWhiteSpace(def.Projectile) ? "Bullet" : def.Projectile;

            EntityFlags targetFlags = EntityFlags.Enemy;

            try
            {
                var parent = world.GetEntity(parentEntityId);
                targetFlags = ResolveTargetFlagsFromOwnerFlags(parent, defaultTargetFlags: EntityFlags.Enemy);
            }
            catch
            {
            }

            var turret = world.CreateEntity();

            turret.Attach(new Transform2(position: Vector2.Zero, scale: new Vector2(2f, 2f)));

            turret.Attach(new TargetingComponent
            {
                MaxRange = def.TargetSearchRangeTiles,
                TargetFlagsMask = targetFlags,
                TargetEntityId = -1
            });

            turret.Attach(new TurretAimComponent
            {
                TurnSpeed = def.TurnSpeed,
                AimOffsetRadians = def.AimOffsetRadians,
                FireAngleToleranceRadians = def.FireAngleToleranceRadians
            });

            turret.Attach(new WeaponComponent(
                projectileArchetype: projectileName,
                cooldownSeconds: def.CooldownSeconds,
                shootSoundKey: def.ShootSound));

            turret.Attach(new BaseStatsComponent(
                turretTurnSpeed: def.TurnSpeed,
                turretCooldownSeconds: def.CooldownSeconds,
                turretRangeTiles: def.TargetSearchRangeTiles));

            turret.Attach(turretSprite);

            turret.Attach(new ChildOfComponent(parentEntityId, localOffset: localOffset, localRotation: localRotation));

            return turret;
        }

        private static EntityFlags ResolveTargetFlagsFromOwnerFlags(Entity owner, EntityFlags defaultTargetFlags)
        {
            if (owner == null)
                return defaultTargetFlags;

            var flags = owner.Get<EntityFlagsComponent>();
            if (flags == null)
                return defaultTargetFlags;

            if (flags.Has(EntityFlags.Enemy))
                return EntityFlags.Player;

            if (flags.Has(EntityFlags.Player))
                return EntityFlags.Enemy;

            return defaultTargetFlags;
        }

        public static Entity CreateEnemyBase(World world, Vector2 position)
        {
            return CreateEnemyBase(world, baseName: DefaultEnemyBaseName, position: position, rotation: 0f);
        }

        public static Entity CreateEnemyBase(World world, string baseName, Vector2 position)
        {
            return CreateEnemyBase(world, baseName: baseName, position: position, rotation: 0f);
        }

        public static Entity CreateEnemyBase(World world, string baseName, Vector2 position, float rotation)
        {
            EnemyBaseDefinition def = ResourceManager.GetEnemyBaseDefinition(baseName);

            string spriteKey = string.IsNullOrWhiteSpace(def.SpriteSheet) ? def.Name : def.SpriteSheet;

            var transform = new Transform2(position: position, rotation: rotation, scale: new Vector2(2f, 2f));

            var sprite = new AnimatedSprite(ResourceManager.GetSpriteSheet(spriteKey), "Idle");
            sprite.Origin = new Vector2(sprite.Size.X / 2f, sprite.Size.Y / 2f);

            var enemyBase = world.CreateEntity();

            enemyBase.Attach(transform);
            enemyBase.Attach(sprite);

            enemyBase.Attach(new EntityFlagsComponent(EntityFlags.Enemy));
            enemyBase.Attach(new WorldPersistentComponent(EntityType.EnemyBase, baseName));

            enemyBase.Attach(new StatMultipliersComponent());
            enemyBase.Attach(new HealthComponent(maxHealth: def.Health));

            if (def.ContactDamage > 0)
                enemyBase.Attach(new DamageComponent(amount: def.ContactDamage));

            enemyBase.Attach(new TerrainCollider(width: def.ColliderWidth, height: def.ColliderHeight, rotates: true));

            enemyBase.Attach(new EntityCollider(
                width: def.ColliderWidth,
                height: def.ColliderHeight,
                layer: CollisionLayer.Enemy,
                collidesWith: CollisionLayer.Player | CollisionLayer.PlayerProjectile,
                isSolid: true,
                rotates: true));

            var turretMounts = new TurretMountsComponent();
            enemyBase.Attach(turretMounts);

            CreateTurretsFromMountDefinitions(world, enemyBase.Id, def.TurretMounts, turretMounts);

            return enemyBase;
        }
        public static Entity CreateProjectile(World world, string projectileName, Vector2 position, float rotation)
        {
            return CreateProjectile(world, projectileName, position, rotation, CollisionLayer.PlayerProjectile, CollisionLayer.Enemy);
        }

        public static Entity CreateProjectile(World world, string projectileName, Vector2 position, float rotation, CollisionLayer layer, CollisionLayer collidesWith)
        {
            ProjectileDefinition def = ResourceManager.GetProjectileDefinition(projectileName);

            string spriteKey = string.IsNullOrWhiteSpace(def.SpriteSheet) ? def.Name : def.SpriteSheet;

            var sprite = new AnimatedSprite(ResourceManager.GetSpriteSheet(spriteKey), "Idle");
            sprite.Origin = new Vector2(sprite.Size.X / 2f, sprite.Size.Y / 2f);

            var transform = new Transform2(position: position, rotation: rotation, scale: new Vector2(2f, 2f));

            var projectile = world.CreateEntity();

            projectile.Attach(transform);
            projectile.Attach(new EntityFlagsComponent(EntityFlags.Projectile));

            projectile.Attach(new MoveStatsComponent
            {
                MaxSpeed = def.Speed,
                RotationSpeed = def.RotationSpeed
            });

            projectile.Attach(new MovementIntent());
            projectile.Attach(new MovementState(transform));
            projectile.Attach(new LifeTimeComponent(def.LifetimeSeconds));

            int colliderSize = def.ColliderSize > 0 ? def.ColliderSize : 1;

            projectile.Attach(new EntityCollider(size: colliderSize, layer: layer, collidesWith: collidesWith, isSolid: false));

            if(def.TerrainExplosionRadiusInTiles >= 0)
                projectile.Attach(new TerrainCollider(size: colliderSize));

            var targeting = new TargetingComponent();

            if (def.IsGuided)
            {
                targeting.MaxRange = def.TargetSearchRangeTiles;
                targeting.TargetFlagsMask = ResolveTargetFlagsFromCollidesWith(collidesWith);
            }

            projectile.Attach(targeting);

            var damage = new DamageComponent(amount: Math.Max(0, def.Damage))
            {
                AreaRadius = Math.Max(0f, def.DamageInAreaRadius),
                AreaDamage = Math.Max(0, def.DamageInAreaDamage),
                TerrainExplosionRadiusInTiles = Math.Max(0, def.TerrainExplosionRadiusInTiles)
            };

            if (damage.Damage > 0 || (damage.AreaRadius > 0f && damage.AreaDamage > 0) || damage.TerrainExplosionRadiusInTiles > 0)
                projectile.Attach(damage);

            projectile.Attach(sprite);

            return projectile;
        }

        private static EntityFlags ResolveTargetFlagsFromCollidesWith(CollisionLayer collidesWith)
        {
            EntityFlags flags = EntityFlags.None;

            if ((collidesWith & CollisionLayer.Player) != 0)
                flags |= EntityFlags.Player;

            if ((collidesWith & CollisionLayer.Enemy) != 0)
                flags |= EntityFlags.Enemy;

            return flags == EntityFlags.None ? EntityFlags.Enemy : flags;
        }

        public static Entity CreateEnemy(World world, string enemyName, Vector2 position, float rotation, int targetEntityId)
        {
            return CreateEnemy(world, enemyName, position, rotation, targetEntityId, scaling: null);
        }

        public static Entity CreateEnemy(World world, string enemyName, Vector2 position, float rotation, int targetEntityId, EnemyScaling scaling)
        {
            EnemyDefinition def = ResourceManager.GetEnemyDefinition(enemyName);

            float healthMultiplier = scaling?.HealthMultiplier ?? 1f;
            float speedMultiplier = scaling?.SpeedMultiplier ?? 1f;
            float damageMultiplier = scaling?.DamageMultiplier ?? 1f;

            int scaledHealth = (int)System.MathF.Round(def.Health * healthMultiplier);
            if (scaledHealth < 1)
                scaledHealth = 1;

            float scaledSpeed = def.MoveSpeed * speedMultiplier;
            if (scaledSpeed < 0f)
                scaledSpeed = 0f;

            int scaledDamage = (int)System.MathF.Round(def.Damage * damageMultiplier);
            if (scaledDamage < 0)
                scaledDamage = 0;

            var enemyTransform = new Transform2(position: position, rotation: rotation, scale: new Vector2(2f, 2f));

            var animEnemy = new AnimatedSprite(ResourceManager.GetSpriteSheet(enemyName), "Idle");
            animEnemy.Origin = new Vector2(animEnemy.Size.X / 2f, animEnemy.Size.Y / 2f);

            var enemy = world.CreateEntity();
            enemy.Attach(enemyTransform);
            enemy.Attach(animEnemy);

            enemy.Attach(new SpriteShadowComponent());

            enemy.Attach(new EntityFlagsComponent(EntityFlags.Enemy));
            enemy.Attach(new WorldPersistentComponent(EntityType.Enemy, enemyName));

            enemy.Attach(new HealthComponent(scaledHealth));
            enemy.Attach(new DamageComponent(scaledDamage));

            enemy.Attach(new MoveStatsComponent
            {
                MaxSpeed = scaledSpeed,
                RotationSpeed = 10f
            });

            enemy.Attach(new TargetingComponent
            {
                TargetEntityId = targetEntityId
            });

            enemy.Attach(new MovementIntent());
            enemy.Attach(new MovementState(enemyTransform));

            int colliderSize = def.ColliderSize > 0 ? def.ColliderSize : 16;
            enemy.Attach(new EntityCollider(colliderSize, CollisionLayer.Enemy, CollisionLayer.Enemy | CollisionLayer.Player | CollisionLayer.PlayerProjectile, true));
            enemy.Attach(new DamageTakenCooldownComponent { CooldownSeconds = 0.25f });

            return enemy;
        }

        public static Entity CreateDeathVfx(World world, string vfxName, Vector2 position)
        {
            if (world == null || string.IsNullOrWhiteSpace(vfxName))
                return null;

            var transform = new Transform2(position: position, rotation: 0f, scale: new Vector2(2f, 2f));

            var anim = new AnimatedSprite(ResourceManager.GetSpriteSheet(vfxName), "Idle");
            anim.Origin = new Vector2(anim.Size.X / 2f, anim.Size.Y / 2f);

            var vfx = world.CreateEntity();
            vfx.Attach(transform);
            vfx.Attach(anim);

            vfx.Attach(new LifeTimeComponent(initialTime: 1.0f));

            return vfx;
        }

        public static Entity CreateAreaExplosionVfx(World world, Vector2 position, float radiusPixels, float durationSeconds, Color color)
        {
            return CreateAreaExplosionVfx(world, position, radiusPixels, durationSeconds, color, thicknessPixels: 8f);
        }

        public static Entity CreateAreaExplosionVfx(World world, Vector2 position, float radiusPixels, float durationSeconds, Color color, float thicknessPixels)
        {
            if (world == null)
                return null;

            if (radiusPixels <= 0f || durationSeconds <= 0f)
                return null;

            var vfx = world.CreateEntity();

            vfx.Attach(new Transform2(position: position, scale: Vector2.One));
            vfx.Attach(new LifeTimeComponent(durationSeconds));

            if (thicknessPixels < 1f)
                thicknessPixels = 1f;

            vfx.Attach(new AreaExplosionVfxComponent
            {
                MaxRadiusPixels = radiusPixels,
                DurationSeconds = durationSeconds,
                Color = color,
                ThicknessPixels = thicknessPixels
            });

            return vfx;
        }

        public static Entity CreateExperienceOrb(World world, Vector2 position, int amount)
        {
            if (world == null)
                return null;

            if (amount <= 0)
                return null;

            string spriteKey = ResourceManager.ResolveExperienceOrbSpriteKey(amount);
            if (string.IsNullOrWhiteSpace(spriteKey))
                spriteKey = DefaultExperienceSprite;

            AnimatedSprite sprite;

            try
            {
                sprite = new AnimatedSprite(ResourceManager.GetSpriteSheet(spriteKey), "Idle");
            }
            catch
            {
                return null;
            }

            sprite.Origin = new Vector2(sprite.Size.X / 2f, sprite.Size.Y / 2f);

            var transform = new Transform2(position: position, rotation: 0f, scale: new Vector2(2f, 2f));

            var orb = world.CreateEntity();
            orb.Attach(transform);
            orb.Attach(sprite);

            orb.Attach(new EntityFlagsComponent(EntityFlags.Pickup));
            orb.Attach(new ExperienceOrbComponent(amount));

            orb.Attach(new MoveStatsComponent(maxSpeed: 750f, acceleration: 1f, rotationSpeed: 0f));
            orb.Attach(new MovementIntent());
            orb.Attach(new MovementState(transform));

            orb.Attach(new EntityCollider(size: 14, layer: CollisionLayer.None, collidesWith: CollisionLayer.None, isSolid: false));
            return orb;
        }

        public static Entity CreateTurretPickup(World world, string turretName, Vector2 position)
        {
            if (world == null || string.IsNullOrWhiteSpace(turretName))
                return null;

            string spriteKey;

            try
            {
                var def = ResourceManager.GetTurretDefinition(turretName);
                spriteKey = string.IsNullOrWhiteSpace(def.SpriteSheet) ? def.Name : def.SpriteSheet;
            }
            catch
            {
                return null;
            }

            AnimatedSprite sprite;

            try
            {
                sprite = new AnimatedSprite(ResourceManager.GetSpriteSheet(spriteKey), "Idle");
            }
            catch
            {
                return null;
            }

            sprite.Origin = new Vector2(sprite.Size.X / 2f, sprite.Size.Y / 2f);

            var transform = new Transform2(position: position, rotation: 0f, scale: new Vector2(2f, 2f));

            var pickup = world.CreateEntity();
            pickup.Attach(transform);
            pickup.Attach(sprite);

            pickup.Attach(new EntityFlagsComponent(EntityFlags.Pickup));
            pickup.Attach(new TurretPickupComponent(turretName));

            pickup.Attach(new MoveStatsComponent(maxSpeed: 750f, acceleration: 1f, rotationSpeed: 0f));
            pickup.Attach(new MovementIntent());
            pickup.Attach(new MovementState(transform));

            pickup.Attach(new EntityCollider(size: 18, layer: CollisionLayer.None, collidesWith: CollisionLayer.None, isSolid: false));
            return pickup;
        }
    }
}