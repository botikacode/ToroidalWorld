using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using System;
using ToroidalWorld.GameEngine;
using ToroidalWorld.GameLogic.Entities.Components;
using ToroidalWorld.GameLogic.Session;

namespace ToroidalWorld.GameLogic.Entities.Systems
{
    public sealed class HealthDeathSystem : EntityProcessingSystem
    {
        private readonly GameSession _session;

        private ComponentMapper<HealthComponent> _healthMapper;
        private ComponentMapper<WorldPersistentComponent> _persistentMapper;
        private ComponentMapper<Transform2> _transformMapper;

        public HealthDeathSystem(GameSession session)
            : base(Aspect.All(typeof(HealthComponent)))
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _healthMapper = mapperService.GetMapper<HealthComponent>();
            _persistentMapper = mapperService.GetMapper<WorldPersistentComponent>();
            _transformMapper = mapperService.GetMapper<Transform2>();
        }

        public override void Process(GameTime gameTime, int entityId)
        {
            var health = _healthMapper.Get(entityId);
            if (health == null)
                return;

            if (health.CurrentHealth > 0)
                return;

            bool isPlayer = entityId == _session.PlayerEntityId;
            if (isPlayer)
            {
                _session.RequestEndSession();
                return;
            }

            var persistent = _persistentMapper.Get(entityId);

            bool isEnemy = persistent != null && persistent.Type == EntityType.Enemy;
            bool isEnemyBase = persistent != null && persistent.Type == EntityType.EnemyBase;

            if (isEnemy)
            {
                TrySpawnEnemyDeathVfx(entityId, persistent);
                TrySpawnEnemyExperience(entityId, persistent);
                _session.Stats.RegisterEnemyKilled();
            }
            else if (isEnemyBase)
            {
                TrySpawnEnemyBaseDeathVfx(entityId, persistent);
                TrySpawnEnemyBaseTurretDrop(entityId, persistent);
            }

            _session.DestroyEntityById(entityId);
        }

        private void TrySpawnEnemyBaseTurretDrop(int entityId, WorldPersistentComponent persistent)
        {
            var transform = _transformMapper.Get(entityId);
            if (transform == null)
                return;

            if (persistent == null || persistent.Type != EntityType.EnemyBase)
                return;

            string baseName = persistent.Archetype;
            if (string.IsNullOrWhiteSpace(baseName))
                return;

            string turretDrop = null;

            try
            {
                var def = ResourceManager.GetEnemyBaseDefinition(baseName);
                turretDrop = def?.TurretDrop;
            }
            catch
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(turretDrop))
                return;

            EntityFactory.CreateTurretPickup(_session.World, turretDrop, transform.Position);
        }

        private void TrySpawnEnemyExperience(int entityId, WorldPersistentComponent persistent)
        {
            var transform = _transformMapper.Get(entityId);
            if (transform == null)
                return;

            if (persistent == null || persistent.Type != EntityType.Enemy)
                return;

            string enemyName = persistent.Archetype;
            if (string.IsNullOrWhiteSpace(enemyName))
                return;

            int amount = 0;

            try
            {
                var def = ResourceManager.GetEnemyDefinition(enemyName);
                amount = def?.ExperienceDrop ?? 0;
            }
            catch
            {
                return;
            }

            if (amount <= 0)
                return;

            EntityFactory.CreateExperienceOrb(_session.World, transform.Position, amount);
        }

        private void TrySpawnEnemyDeathVfx(int entityId, WorldPersistentComponent persistent)
        {
            var transform = _transformMapper.Get(entityId);
            if (transform == null)
                return;

            if (persistent == null || persistent.Type != EntityType.Enemy)
                return;

            string enemyName = persistent.Archetype;
            if (string.IsNullOrWhiteSpace(enemyName))
                return;

            string deathVfx = null;

            try
            {
                var def = ResourceManager.GetEnemyDefinition(enemyName);
                deathVfx = def?.DeathVfx;
            }
            catch
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(deathVfx))
                return;

            EntityFactory.CreateDeathVfx(_session.World, deathVfx, transform.Position);
        }

        private void TrySpawnEnemyBaseDeathVfx(int entityId, WorldPersistentComponent persistent)
        {
            var transform = _transformMapper.Get(entityId);
            if (transform == null)
                return;

            if (persistent == null || persistent.Type != EntityType.EnemyBase)
                return;

            string baseName = persistent.Archetype;
            if (string.IsNullOrWhiteSpace(baseName))
                return;

            string deathVfx = null;

            try
            {
                var def = ResourceManager.GetEnemyBaseDefinition(baseName);
                deathVfx = def?.DeathVfx;
            }
            catch
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(deathVfx))
                return;

            EntityFactory.CreateDeathVfx(_session.World, deathVfx, transform.Position);
        }
    }
}