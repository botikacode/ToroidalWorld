using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace ToroidalWorld.GameLogic.Physics
{
    public sealed class DerivedCollisionEvents
    {
        public List<ProjectileImpactEvent> ProjectileImpacts { get; } = new List<ProjectileImpactEvent>(128);

        public List<ContactDamageEvent> ContactDamages { get; } = new List<ContactDamageEvent>(128);

        public List<TerrainCollisionEvent> TerrainCollisions { get; } = new List<TerrainCollisionEvent>(128);
    }

    public readonly struct ProjectileImpactEvent
    {
        public int ProjectileId { get; }
        public int OtherEntityId { get; } // if the projectile hit an entity, otherwise -1
        public Point WorldTile { get; } // only valid if HasWorldTile is true
        public bool HasWorldTile { get; }
        public Vector2 ImpactWorldPosition { get; }

        public ProjectileImpactEvent(int projectileId, int otherEntityId, Vector2 impactWorldPosition)
        {
            ProjectileId = projectileId;
            OtherEntityId = otherEntityId;
            ImpactWorldPosition = impactWorldPosition;

            WorldTile = default;
            HasWorldTile = false;
        }

        public ProjectileImpactEvent(int projectileId, Point worldTile, Vector2 impactWorldPosition)
        {
            ProjectileId = projectileId;
            OtherEntityId = -1;
            WorldTile = worldTile;
            HasWorldTile = true;
            ImpactWorldPosition = impactWorldPosition;
        }
    }

    public readonly struct ContactDamageEvent
    {
        public int AttackerId { get; }
        public int VictimId { get; }

        public ContactDamageEvent(int attackerId, int victimId)
        {
            AttackerId = attackerId;
            VictimId = victimId;
        }
    }

    public readonly struct TerrainCollisionEvent
    {
        public int EntityId { get; }
        public Point WorldTile { get; }
        public Vector2 ImpactWorldPosition { get; }

        public TerrainCollisionEvent(int entityId, Point worldTile, Vector2 impactWorldPosition)
        {
            EntityId = entityId;
            WorldTile = worldTile;
            ImpactWorldPosition = impactWorldPosition;
        }
    }
}