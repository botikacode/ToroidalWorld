using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using ToroidalWorld.GameLogic.Entities.Components;
using ToroidalWorld.GameLogic.Map;
using ToroidalWorld.GameLogic.Physics;
using ToroidalWorld.GameLogic.Session;

namespace ToroidalWorld.GameLogic.Entities.Systems
{
    public sealed class ProjectileImpactProcessingSystem : UpdateSystem
    {
        private readonly GameSession _session;
        private readonly MapData _mapData;
        private readonly DerivedCollisionEvents _derived;

        private readonly HashSet<int> _projectilesToDestroy = new HashSet<int>();
        private readonly HashSet<int> _entitiesToDestroy = new HashSet<int>();

        public ProjectileImpactProcessingSystem(GameSession session, MapData mapData, DerivedCollisionEvents derived)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _mapData = mapData ?? throw new ArgumentNullException(nameof(mapData));
            _derived = derived ?? throw new ArgumentNullException(nameof(derived));
        }

        public override void Update(GameTime gameTime)
        {
            _projectilesToDestroy.Clear();
            _entitiesToDestroy.Clear();

            for (int i = 0; i < _derived.ProjectileImpacts.Count; i++)
            {
                var evt = _derived.ProjectileImpacts[i];

                if (!TryGetEntity(evt.ProjectileId, out var projectile))
                    continue;

                if (_projectilesToDestroy.Contains(evt.ProjectileId))
                    continue;

                ApplyDirectDamage(projectile, evt.OtherEntityId);
                ApplyDamageInArea(projectile, evt.ImpactWorldPosition);
                ApplyTerrainExplosion(projectile, evt);

                TrySpawnAreaExplosionVfx(projectile, evt.ImpactWorldPosition);

                _projectilesToDestroy.Add(evt.ProjectileId);
            }

            foreach (int entityId in _entitiesToDestroy)
            {
                try { _session.DestroyEntityById(entityId); }
                catch { }
            }

            foreach (int projectileId in _projectilesToDestroy)
            {
                try { _session.DestroyEntityById(projectileId); }
                catch { }
            }

            _derived.ProjectileImpacts.Clear();
        }

        private void TrySpawnAreaExplosionVfx(Entity projectile, Vector2 centerWorld)
        {
            float radiusPixels = 0f;

            var damage = projectile.Get<DamageComponent>();
            if (damage != null)
            {
                if (damage.AreaRadius > 0f && damage.AreaDamage > 0)
                    radiusPixels = MathF.Max(radiusPixels, damage.AreaRadius);

                if (damage.TerrainExplosionRadiusInTiles > 0)
                    radiusPixels = MathF.Max(radiusPixels, damage.TerrainExplosionRadiusInTiles * MapData.PixelSize);
            }

            if (radiusPixels <= 0f)
                return;

            EntityFactory.CreateAreaExplosionVfx(
                _session.World,
                centerWorld,
                radiusPixels,
                durationSeconds: 0.25f,
                color: Color.Orange);
        }

        private void ApplyDirectDamage(Entity projectile, int otherEntityId)
        {
            if (otherEntityId < 0)
                return;

            if (!TryGetEntity(otherEntityId, out var other))
                return;

            var damage = projectile.Get<DamageComponent>();
            var health = other.Get<HealthComponent>();

            if (damage == null || health == null)
                return;

            if (damage.Damage <= 0)
                return;

            health.CurrentHealth -= damage.Damage;
            if (health.CurrentHealth <= 0)
                _entitiesToDestroy.Add(otherEntityId);
        }

        private void ApplyDamageInArea(Entity projectile, Vector2 centerWorld)
        {
            var damage = projectile.Get<DamageComponent>();
            if (damage == null || damage.AreaRadius <= 0f || damage.AreaDamage <= 0)
                return;

            float radiusSq = damage.AreaRadius * damage.AreaRadius;

            Point centerChunk = _mapData.GetChunkIdFromWorldCoords((int)centerWorld.X, (int)centerWorld.Y);

            int chunkWorldSize = MapData.ChunkSize * MapData.PixelSize;
            int chunkRadius = (int)Math.Ceiling(damage.AreaRadius / chunkWorldSize);

            for (int dx = -chunkRadius; dx <= chunkRadius; dx++)
            {
                for (int dy = -chunkRadius; dy <= chunkRadius; dy++)
                {
                    Point chunkId = _mapData.WrapChunkCoordinates(centerChunk.X + dx, centerChunk.Y + dy);

                    var candidates = new List<int>(_mapData.GetEntitiesInChunk(chunkId));

                    for (int i = 0; i < candidates.Count; i++)
                    {
                        int entityId = candidates[i];

                        if (!TryGetEntity(entityId, out var entity))
                            continue;

                        var health = entity.Get<HealthComponent>();
                        var transform = entity.Get<Transform2>();
                        if (health == null || transform == null)
                            continue;

                        Vector2 pos = _mapData.GetToroidalPosition(transform.Position, centerWorld);
                        float distSq = Vector2.DistanceSquared(pos, centerWorld);

                        if (distSq > radiusSq)
                            continue;

                        health.CurrentHealth -= damage.AreaDamage;
                        if (health.CurrentHealth <= 0)
                            _entitiesToDestroy.Add(entityId);
                    }
                }
            }
        }

        private void ApplyTerrainExplosion(Entity projectile, ProjectileImpactEvent evt)
        {
            var damage = projectile.Get<DamageComponent>();
            if (damage == null || damage.TerrainExplosionRadiusInTiles <= 0)
                return;

            Point tile;

            if (evt.HasWorldTile)
            {
                tile = evt.WorldTile;
            }
            else
            {
                int tileX = (int)(evt.ImpactWorldPosition.X / MapData.PixelSize);
                int tileY = (int)(evt.ImpactWorldPosition.Y / MapData.PixelSize);
                tile = new Point(tileX, tileY);
            }

            _mapData.ApplyExplosion(tile, damage.TerrainExplosionRadiusInTiles);
        }

        private bool TryGetEntity(int entityId, out Entity entity)
        {
            entity = null;

            try { entity = _session.World.GetEntity(entityId); }
            catch { return false; }

            return entity != null;
        }
    }
}