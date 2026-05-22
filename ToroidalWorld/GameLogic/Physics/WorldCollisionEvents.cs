using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace ToroidalWorld.GameLogic.Physics
{
    public sealed class WorldCollisionEvents
    {
        public List<WorldCollisionEvent> WorldCollisions { get; } = new List<WorldCollisionEvent>(128);
    }

    public readonly struct WorldCollisionEvent
    {
        public int EntityId { get; }
        public Point MapPosition { get; }

        public WorldCollisionEvent(int entityId, Point mapPosition)
        {
            EntityId = entityId;
            MapPosition = mapPosition;
        }
    }
}