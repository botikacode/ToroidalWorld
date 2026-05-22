using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.ECS.Systems;
using System;
using ToroidalWorld.GameLogic.Entities.Components;
using ToroidalWorld.GameLogic.Map;
using ToroidalWorld.GameLogic.Physics.Components;
using ToroidalWorld.GameLogic.Session;

namespace ToroidalWorld.GameLogic.Physics.Systems
{
    public sealed class CollisionEventRouterSystem : UpdateSystem
    {
        private readonly GameSession _session;
        private readonly MapData _mapData;
        private readonly EntityCollisionEvents _entityCollisionEvents;
        private readonly WorldCollisionEvents _worldCollisionEvents;
        private readonly DerivedCollisionEvents _derived;

        public CollisionEventRouterSystem(
            GameSession session,
            MapData mapData,
            EntityCollisionEvents entityCollisionEvents,
            WorldCollisionEvents worldCollisionEvents,
            DerivedCollisionEvents derived)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _mapData = mapData ?? throw new ArgumentNullException(nameof(mapData));
            _entityCollisionEvents = entityCollisionEvents ?? throw new ArgumentNullException(nameof(entityCollisionEvents));
            _worldCollisionEvents = worldCollisionEvents ?? throw new ArgumentNullException(nameof(worldCollisionEvents));
            _derived = derived ?? throw new ArgumentNullException(nameof(derived));
        }

        public override void Update(GameTime gameTime)
        {
            _derived.ProjectileImpacts.Clear();
            _derived.ContactDamages.Clear();
            _derived.TerrainCollisions.Clear();

            RouteEntityCollisions();
            RouteWorldCollisions();

            _entityCollisionEvents.EntityCollisions.Clear();
            _worldCollisionEvents.WorldCollisions.Clear();
        }

        private void RouteEntityCollisions()
        {
            for (int i = 0; i < _entityCollisionEvents.EntityCollisions.Count; i++)
            {
                var evt = _entityCollisionEvents.EntityCollisions[i];

                if (!TryGetEntity(evt.EntityAId, out var a) || !TryGetEntity(evt.EntityBId, out var b))
                    continue;

                bool aIsProjectile = HasFlag(a, EntityFlags.Projectile);
                bool bIsProjectile = HasFlag(b, EntityFlags.Projectile);

                // Projectile vs entity
                if (aIsProjectile)
                {
                    if (TryGetTransform(a, out var projectileTransform))
                        _derived.ProjectileImpacts.Add(new ProjectileImpactEvent(a.Id, b.Id, projectileTransform.Position));
                }
                else if (bIsProjectile)
                {
                    if (TryGetTransform(b, out var projectileTransform))
                        _derived.ProjectileImpacts.Add(new ProjectileImpactEvent(b.Id, a.Id, projectileTransform.Position));
                }

                // Contact damage: entity with DamageComponent (and NOT a projectile) <-> player
                bool aIsPlayer = HasFlag(a, EntityFlags.Player);
                bool bIsPlayer = HasFlag(b, EntityFlags.Player);

                bool aDealsContact = !aIsProjectile && a.Get<DamageComponent>() != null;
                bool bDealsContact = !bIsProjectile && b.Get<DamageComponent>() != null;

                if (aDealsContact && bIsPlayer)
                    _derived.ContactDamages.Add(new ContactDamageEvent(attackerId: a.Id, victimId: b.Id));
                else if (bDealsContact && aIsPlayer)
                    _derived.ContactDamages.Add(new ContactDamageEvent(attackerId: b.Id, victimId: a.Id));
            }
        }

        private void RouteWorldCollisions()
        {
            for (int i = 0; i < _worldCollisionEvents.WorldCollisions.Count; i++)
            {
                var evt = _worldCollisionEvents.WorldCollisions[i];

                if (!TryGetEntity(evt.EntityId, out var entity))
                    continue;

                if (!TryGetTransform(entity, out var transform))
                    continue;

                // Maintain existing route for projectiles
                if (HasFlag(entity, EntityFlags.Projectile))
                {
                    var wrappedTileWorldPx = _mapData.WrapWorldCoordinates(evt.MapPosition.X, evt.MapPosition.Y);

                    var impactWorldPosition = new Vector2(
                        wrappedTileWorldPx.X + (MapData.PixelSize * 0.5f),
                        wrappedTileWorldPx.Y + (MapData.PixelSize * 0.5f));

                    _derived.ProjectileImpacts.Add(new ProjectileImpactEvent(entity.Id, evt.MapPosition, impactWorldPosition));
                    continue;
                }

                _derived.TerrainCollisions.Add(new TerrainCollisionEvent(entity.Id, evt.MapPosition, transform.Position));
            }
        }

        private static bool HasFlag(MonoGame.Extended.ECS.Entity entity, EntityFlags flags)
        {
            var f = entity.Get<EntityFlagsComponent>();
            return f != null && f.Has(flags);
        }

        private bool TryGetEntity(int entityId, out MonoGame.Extended.ECS.Entity entity)
        {
            entity = null;
            try { entity = _session.World.GetEntity(entityId); }
            catch { return false; }
            return entity != null;
        }

        private static bool TryGetTransform(MonoGame.Extended.ECS.Entity entity, out Transform2 transform)
        {
            transform = entity.Get<Transform2>();
            return transform != null;
        }
    }
}