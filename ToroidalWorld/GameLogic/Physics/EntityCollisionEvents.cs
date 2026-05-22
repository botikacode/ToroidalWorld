using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToroidalWorld.GameLogic.Physics
{
    public sealed class EntityCollisionEvents
    {
        public List<EntityCollisionEvent> EntityCollisions { get; } = new List<EntityCollisionEvent>(128);
    }

    public readonly struct EntityCollisionEvent
    {
        public int EntityAId { get; }
        public int EntityBId { get; }

        public EntityCollisionEvent(int entityAId, int entityBId)
        {
            EntityAId = entityAId;
            EntityBId = entityBId; 
        }
    }
}
