using System;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using ToroidalWorld.GameEngine;
using ToroidalWorld.GameLogic.Components;
using ToroidalWorld.GameLogic.Entities;
using ToroidalWorld.GameLogic.Entities.Components;
using ToroidalWorld.GameLogic.Map;
using ToroidalWorld.GameLogic.Physics;
using ToroidalWorld.GameLogic.Session;

namespace ToroidalWorld.GameLogic.Entities.Systems
{
    public sealed class TurretFireSystem : EntityProcessingSystem
    {
        private readonly GameSession _gameSession;
        private readonly MapData _map;

        private ComponentMapper<WeaponComponent> _weaponMapper;
        private ComponentMapper<ChildOfComponent> _childOfMapper;
        private ComponentMapper<TargetingComponent> _targetingMapper;
        private ComponentMapper<TurretAimComponent> _turretAimMapper;
        private ComponentMapper<Transform2> _transformMapper;

        public TurretFireSystem(GameSession gameSession, MapData map)
            : base(Aspect.All(typeof(WeaponComponent), typeof(TargetingComponent), typeof(TurretAimComponent), typeof(Transform2), typeof(ChildOfComponent)))
        {
            _gameSession = gameSession ?? throw new ArgumentNullException(nameof(gameSession));
            _map = map ?? throw new ArgumentNullException(nameof(map));
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _weaponMapper = mapperService.GetMapper<WeaponComponent>();
            _childOfMapper = mapperService.GetMapper<ChildOfComponent>();
            _targetingMapper = mapperService.GetMapper<TargetingComponent>();
            _turretAimMapper = mapperService.GetMapper<TurretAimComponent>();
            _transformMapper = mapperService.GetMapper<Transform2>();
        }

        public override void Process(GameTime gameTime, int entityId)
        {
            var weapon = _weaponMapper.Get(entityId);
            var targeting = _targetingMapper.Get(entityId);
            var turretAim = _turretAimMapper.Get(entityId);
            var transform = _transformMapper.Get(entityId);
            var childOf = _childOfMapper.Get(entityId);

            if (weapon == null || targeting == null || turretAim == null || transform == null || childOf == null)
                return;

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            weapon.RemainingSeconds -= dt;
            if (weapon.RemainingSeconds > 0f)
                return;

            if (targeting.TargetEntityId < 0)
                return;

            Transform2 targetTransform;

            try
            {
                targetTransform = _gameSession.World.GetEntity(targeting.TargetEntityId)?.Get<Transform2>();
            }
            catch
            {
                return;
            }

            if (targetTransform == null)
                return;

            var turretWorldPos = transform.Position;
            var targetWorldPos = _map.GetToroidalPosition(targetTransform.Position, turretWorldPos);

            var desiredRotation =
                (float)Math.Atan2(targetWorldPos.Y - turretWorldPos.Y, targetWorldPos.X - turretWorldPos.X)
                + MathHelper.PiOver2
                + turretAim.AimOffsetRadians;

            var aimError = Math.Abs(MathHelper.WrapAngle(desiredRotation - transform.Rotation));
            if (aimError > turretAim.FireAngleToleranceRadians)
                return;

            weapon.RemainingSeconds = weapon.CooldownSeconds;

            CollisionLayer projectileLayer = CollisionLayer.PlayerProjectile;
            CollisionLayer projectileCollidesWith = CollisionLayer.Enemy;
            StatMultipliersComponent multipliers = null;

            try
            {
                var owner = _gameSession.World.GetEntity(childOf.ParentEntityId);

                ResolveProjectileLayersFromOwnerFlags(owner, out projectileLayer, out projectileCollidesWith);

                multipliers = owner?.Get<StatMultipliersComponent>();
            }
            catch
            {
            }

            var projectile = EntityFactory.CreateProjectile(
                _gameSession.World,
                weapon.ProjectileArchetype,
                transform.Position,
                transform.Rotation,
                projectileLayer,
                projectileCollidesWith);

            if (multipliers != null)
                ApplyMultipliers(projectile, multipliers);

            if (!string.IsNullOrWhiteSpace(weapon.ShootSoundKey))
                AudioManager.TryPlaySoundEffect(weapon.ShootSoundKey, volume: 0.8f);
        }

        private static void ResolveProjectileLayersFromOwnerFlags(Entity owner, out CollisionLayer projectileLayer, out CollisionLayer projectileCollidesWith)
        {
            projectileLayer = CollisionLayer.PlayerProjectile;
            projectileCollidesWith = CollisionLayer.Enemy;

            if (owner == null)
                return;

            var flags = owner.Get<EntityFlagsComponent>();
            if (flags == null)
                return;

            if (flags.Has(EntityFlags.Enemy))
            {
                projectileLayer = CollisionLayer.EnemyProjectile;
                projectileCollidesWith = CollisionLayer.Player;
            }
            else
            {
                projectileLayer = CollisionLayer.PlayerProjectile;
                projectileCollidesWith = CollisionLayer.Enemy;
            }
        }

        private static void ApplyMultipliers(Entity projectile, StatMultipliersComponent mult)
        {
            var damage = projectile.Get<DamageComponent>();
            if (damage == null)
                return;

            if (mult.DamageMultiplier > 0f)
                damage.Damage = Math.Max(0, (int)MathF.Round(damage.Damage * mult.DamageMultiplier));

            if (damage.AreaRadius > 0f && damage.AreaDamage > 0)
            {
                if (mult.AreaDamageMultiplier > 0f)
                    damage.AreaDamage = Math.Max(0, (int)MathF.Round(damage.AreaDamage * mult.AreaDamageMultiplier));

                if (mult.AreaRadiusMultiplier > 0f)
                    damage.AreaRadius = Math.Max(0f, damage.AreaRadius * mult.AreaRadiusMultiplier);
            }
        }
    }
}