using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.Graphics;
using System;
using ToroidalWorld.GameEngine;
using ToroidalWorld.GameLogic.Components;
using ToroidalWorld.GameLogic.Entities;
using ToroidalWorld.GameLogic.Entities.Components;
using ToroidalWorld.GameLogic.Entities.Systems;
using ToroidalWorld.GameLogic.Map;
using ToroidalWorld.GameLogic.Map.Systems;
using ToroidalWorld.GameLogic.Physics;
using ToroidalWorld.GameLogic.Physics.Components;
using ToroidalWorld.GameLogic.Physics.Systems;
using ToroidalWorld.GameLogic.Systems;

namespace ToroidalWorld.GameLogic.Session
{
    public sealed class GameSession : IDisposable
    {
        public GameSession(
            string worldName,
            int seed,
            OrthographicCamera camera,
            GraphicsDevice graphicsDevice,
            SpriteBatch spriteBatch,
            ContentManager content,
            string playerBoatName)
        {
            Stats.Reset();

            Map = new MapData(seed, worldName);
            EntityCollisionEvents = new EntityCollisionEvents();
            WorldCollisionEvents = new WorldCollisionEvents();
            Camera = camera;
            GraphicsDevice = graphicsDevice;
            SpriteBatch = spriteBatch;
            Content = content;

            BuildWorld();

            if (string.IsNullOrWhiteSpace(playerBoatName))
                playerBoatName = "Boat-001";

            var player = EntityFactory.CreatePlayer(World, boatName: playerBoatName, position: new Vector2(0, 0), scale: new Vector2(2f, 2f));
            PlayerEntityId = player.Id;

        }

        public GameSession(
            string worldName,
            int seed,
            OrthographicCamera camera,
            GraphicsDevice graphicsDevice,
            SpriteBatch spriteBatch,
            ContentManager content,
            MonoGame.Extended.Screens.ScreenManager screenManager,
            Action<float> setPlayerHealthBarPercent = null)
            : this(worldName, seed, camera, graphicsDevice, spriteBatch, content, playerBoatName: "Boat-001")
        {
            _ = screenManager;
            _ = setPlayerHealthBarPercent;
        }

        public bool IsWorldReady { get; internal set; }

        public int WorldReadyChunks { get; internal set; }

        public int WorldTotalChunks { get; internal set; }

        public GameSessionStats Stats { get; } = new GameSessionStats();

        public bool HasEnded { get; private set; }

        public ContentManager Content { get; }

        public World World { get; private set; }
        public MapData Map { get; }
        public EntityCollisionEvents EntityCollisionEvents { get; }
        public WorldCollisionEvents WorldCollisionEvents { get; }
        private DerivedCollisionEvents _derivedCollisionEvents { get; } = new DerivedCollisionEvents();
        public OrthographicCamera Camera { get; }
        public GraphicsDevice GraphicsDevice { get; }
        public SpriteBatch SpriteBatch { get; }

        public int PlayerEntityId { get; private set; } = -1;

        public bool HasPlayer => PlayerEntityId >= 0;

        public bool IsInitialized => World != null;

        public TurretPickupSelectionState PendingTurretPickup { get; private set; }

        public bool HasPendingTurretPickup => PendingTurretPickup != null;

        /// <summary>
        /// Finalize the session: freezes the world (no longer updates) but allows it to be drawn
        /// to show overlays (GameOver/Pause) from the same `GameplayScreen`.
        /// The actual release of the `World` is done in `Dispose()`.
        /// </summary>
        public void RequestEndSession()
        {
            if (HasEnded)
                return;

            HasEnded = true;

            // Stop SFX loop (IceBreakLoop).
            AudioManager.StopManagedLoopingSoundEffects(disposeInstances: true);

            AudioManager.StopMusic();
            AudioManager.TryPlaySoundEffect("PlayerDeath");

            PlayerEntityId = -1;
        }

        public void AttachWorld(World world)
        {
            World = world;
        }

        public void SetPlayer(Entity playerEntity)
        {
            PlayerEntityId = playerEntity.Id;
        }

        public Entity GetPlayerEntity()
        {
            if (!HasPlayer)
                return null;

            return World.GetEntity(PlayerEntityId);
        }

        public bool TryGetPlayerTransform(out Transform2 transform)
        {
            transform = null;

            if (!HasPlayer)
                return false;

            Entity player = null;

            try
            {
                player = World.GetEntity(PlayerEntityId);
            }
            catch
            {
                return false;
            }

            if (player == null)
                return false;

            transform = player.Get<Transform2>();
            return transform != null;
        }

        public bool TrySwapPlayerTurret(int mountIndex, string turretName)
        {
            if (World == null || !HasPlayer)
                return false;

            Entity player;

            try
            {
                player = World.GetEntity(PlayerEntityId);
            }
            catch
            {
                return false;
            }

            if (player == null)
                return false;

            var mounts = player.Get<TurretMountsComponent>();
            if (mounts == null)
                return false;

            if (mountIndex < 0 || mountIndex >= mounts.Mounts.Count)
                return false;

            var mount = mounts.Mounts[mountIndex];
            if (mount == null)
                return false;

            if (mount.TurretEntityId >= 0)
            {
                try
                {
                    World.DestroyEntity(mount.TurretEntityId);
                }
                catch
                {
                }
            }

            var turret = EntityFactory.CreateTurret(
                World,
                turretName,
                parentEntityId: PlayerEntityId,
                localOffset: mount.LocalOffset,
                localRotation: mount.LocalRotationRadians);

            mount.TurretEntityId = turret.Id;
            mount.TurretName = turretName;

            return true;
        }

        public void Update(GameTime gameTime)
        {
            if (IsWorldReady && !AudioManager.IsMusicPlaying() && !HasEnded)
                AudioManager.TryPlaySong("1_stomper");

            if (HasEnded)
                return;

            World?.Update(gameTime);
        }

        public void Draw(GameTime gameTime)
        {
            World?.Draw(gameTime);
        }

        public void Dispose()
        {
            AudioManager.StopManagedLoopingSoundEffects(disposeInstances: true);

            try
            {
                World?.Dispose();
            }
            catch
            {
            }

            World = null;
            PlayerEntityId = -1;
        }

        private void BuildWorld()
        {
            World = new WorldBuilder()

                // Map Systems
                .AddSystem(new VisibleChunkRequestSystem(Map, Camera))
                .AddSystem(new ChunkGenerationSystem(Map))

                .AddSystem(new PlayerInitialWaterSpawnSystem(this, Map))

                .AddSystem(new ChunkRenderBuildSystem(Map, GraphicsDevice))
                .AddSystem(new ChunkRenderSystem(Map, SpriteBatch, Camera))

                .AddSystem(new ChunkActiveEntitySystem(Map, Camera, this))
                .AddSystem(new ChunkActiveTransientEntitySystem(Map, Camera, this))
                .AddSystem(new ChunkSpawnEntitySystem(Map, Camera, this))

                .AddSystem(new ToroidalEntityWarpSystem(this, Map))

                // Movement and Collision Systems
                .AddSystem(new PlayerStatsApplySystem())
                .AddSystem(new PlayerHealthRegenSystem())
                .AddSystem(new InputSystem(this))
                .AddSystem(new SeekTargetSteeringSystem(this))
                .AddSystem(new HomingProjectileSteeringSystem(this))
                .AddSystem(new MovementAnimationSystem())
                .AddSystem(new MovementSystem())
                .AddSystem(new ExperienceOrbMagnetSystem(this, Map))
                .AddSystem(new TurretPickupMagnetSystem(this, Map))


                // Pre-collision: resistance + breaks ahead
                .AddSystem(new IcebreakerProbeSystem(Map))

                .AddSystem(new EntityCollisionSystem(Map, EntityCollisionEvents))
                .AddSystem(new WorldCollisionSystem(Map, WorldCollisionEvents))
                .AddSystem(new CollisionEventRouterSystem(this, Map, EntityCollisionEvents, WorldCollisionEvents, _derivedCollisionEvents))

                // Post-collision: your current IcebreakerSystem (wedge + cleanup inside collider)
                .AddSystem(new IcebreakerSystem(this, Map, _derivedCollisionEvents))

                .AddSystem(new ContactDamageProcessingSystem(this, _derivedCollisionEvents))
                .AddSystem(new ProjectileImpactProcessingSystem(this, Map, _derivedCollisionEvents))
                .AddSystem(new HealthDeathSystem(this))
                .AddSystem(new TransformUpdateSystem())
                .AddSystem(new TurretPickupSpinSystem())
                .AddSystem(new TurretPickupMarkerVfxSystem(this))


                .AddSystem(new WaterWakeTrailUpdateSystem())

                .AddSystem(new TargetSearchSystem(this, Map))
                .AddSystem(new AimAtTargetSystem(this, Map))
                .AddSystem(new ParentChildTransformSystem(this))
                .AddSystem(new PlayerCameraFollowAndShakeSystem(this))

                // Entity-specific Systems
                .AddSystem(new PlayerTurretSwapDebugSystem(this))

                .AddSystem(new TurretFireSystem(this, Map))
                .AddSystem(new LifeTimeSystem(this))
                .AddSystem(new StageSpawnerSystem(this, stageId: "stage_01"))
                .AddSystem(new StageEnemyBaseSpawnerSystem(this, Map, stageId: "stage_01"))


                // Render Systems
                .AddSystem(new AnimatedSpriteUpdateSystem())

                .AddSystem(new WaterWakeTrailRenderSystem(SpriteBatch, Camera, GraphicsDevice))
                .AddSystem(new SpriteShadowRenderSystem(SpriteBatch, Camera))

                .AddSystem(new AnimatedSpriteRenderSystem(SpriteBatch, Camera))
                .AddSystem(new AreaExplosionVfxRenderSystem(SpriteBatch, Camera, GraphicsDevice))

                // Loading Systems
                .AddSystem(new WorldLoadingStatusSystem(this, Map))

                .AddSystem(new EnemyBaseDebugSpawnSystem(this, Map, baseName: "EnemyBase-001"))

                .AddSystem(new WorldLoadingOverlayRenderSystem(this, SpriteBatch, Content.Load<SpriteFont>("Fonts/Pixelbasel")))

                // Debug Systems
                //.AddSystem(new FPSViewerRenderSystem(SpriteBatch, Content.Load<SpriteFont>("Fonts/Pixelbasel")))
                //.AddSystem(new PlayerHealthDebugRenderSystem(this, SpriteBatch, Content.Load<SpriteFont>("Fonts/Pixelbasel")))
                //.AddSystem(new StageStatsDebugRenderSystem(this, SpriteBatch, Content.Load<SpriteFont>("Fonts/Pixelbasel")))
                //.AddSystem(new CollisionDebugSystem(this, SpriteBatch, Camera, GraphicsDevice))
                .Build();
        }

        public void DestroyEntityById(int entityId)
        {
            if (World == null || entityId < 0)
                return;

            DestroyEntityHierarchyById(entityId, depth: 0);
        }

        private void DestroyEntityHierarchyById(int entityId, int depth)
        {
            if (World == null || entityId < 0)
                return;

            if (depth > 16)
                return;

            Entity entity = null;

            try
            {
                entity = World.GetEntity(entityId);
            }
            catch
            {
                return;
            }

            if (entity == null)
                return;

            TurretMountsComponent mounts = null;

            try
            {
                mounts = entity.Get<TurretMountsComponent>();
            }
            catch
            {
            }

            if (mounts != null)
            {
                for (int i = 0; i < mounts.Mounts.Count; i++)
                {
                    var mount = mounts.Mounts[i];
                    if (mount == null)
                        continue;

                    int turretId = mount.TurretEntityId;
                    if (turretId >= 0)
                    {
                        DestroyEntityHierarchyById(turretId, depth + 1);
                        mount.TurretEntityId = -1;
                    }
                }
            }

            Transform2 transform = null;

            try
            {
                transform = entity.Get<Transform2>();
            }
            catch
            {
            }

            if (transform != null)
            {
                Point chunkId = Map.GetChunkIdFromWorldCoords((int)transform.Position.X, (int)transform.Position.Y);
                Map.RemoveEntityFromChunk(chunkId, entityId);
            }

            try
            {
                World.DestroyEntity(entityId);
            }
            catch
            {
            }
        }

        public bool TryGetPlayerEntity(out Entity player)
        {
            player = null;

            if (!HasPlayer || World == null)
                return false;

            try
            {
                player = World.GetEntity(PlayerEntityId);
            }
            catch
            {
                return false;
            }

            return player != null;
        }

        public bool TryGetPlayerHealth(out int currentHealth, out int maxHealth)
        {
            currentHealth = 0;
            maxHealth = 0;

            if (!TryGetPlayerEntity(out var player))
                return false;

            HealthComponent health = null;

            try
            {
                health = player.Get<HealthComponent>();
            }
            catch
            {
                return false;
            }

            if (health == null)
                return false;

            currentHealth = health.CurrentHealth;
            maxHealth = health.MaxHealth;
            return true;
        }

        public bool TryGetPlayerLevel(out int level, out int experience, out int experienceToNextLevel, out int pendingLevelUps)
        {
            level = 0;
            experience = 0;
            experienceToNextLevel = 0;
            pendingLevelUps = 0;

            if (!TryGetPlayerEntity(out var player))
                return false;

            PlayerLevelComponent playerLevel = null;

            try
            {
                playerLevel = player.Get<PlayerLevelComponent>();
            }
            catch
            {
                return false;
            }

            if (playerLevel == null)
                return false;

            level = playerLevel.Level;
            experience = playerLevel.Experience;
            experienceToNextLevel = playerLevel.ExperienceToNextLevel;
            pendingLevelUps = playerLevel.PendingLevelUps;
            return true;
        }

        public int GetEnemiesKilled()
        {
            return Stats?.EnemiesKilled ?? 0;
        }

        public bool TryBeginTurretPickupSelection(int pickupEntityId, string turretName)
        {
            if (World == null)
                return false;

            if (pickupEntityId < 0 || string.IsNullOrWhiteSpace(turretName))
                return false;

            if (PendingTurretPickup != null)
                return false;

            PendingTurretPickup = new TurretPickupSelectionState
            {
                PickupEntityId = pickupEntityId,
                TurretName = turretName
            };

            return true;
        }
        public void CancelTurretPickupSelection(bool destroyPickup)
        {
            if (PendingTurretPickup == null)
                return;

            int pickupId = PendingTurretPickup.PickupEntityId;

            PendingTurretPickup = null;

            if (destroyPickup && pickupId >= 0)
                DestroyEntityById(pickupId);
        }

        public bool TryConfirmTurretPickupSelection(int mountIndex)
        {
            if (PendingTurretPickup == null)
                return false;

            int pickupId = PendingTurretPickup.PickupEntityId;
            string turretName = PendingTurretPickup.TurretName;

            PendingTurretPickup = null;

            bool swapped = TrySwapPlayerTurret(mountIndex, turretName);

            if (swapped && pickupId >= 0)
                DestroyEntityById(pickupId);

            return swapped;
        }

    }
}