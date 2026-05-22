using System;

namespace ToroidalWorld.GameLogic.Entities.Components
{
    [Flags]
    public enum EntityFlags
    {
        None = 0,
        Player = 1 << 0,
        Enemy = 1 << 1,
        Projectile = 1 << 2,
        Pickup = 1 << 3,
    }

    public sealed class EntityFlagsComponent
    {
        public EntityFlagsComponent(EntityFlags flags)
        {
            Flags = flags;
        }

        public EntityFlags Flags { get; set; }

        public bool Has(EntityFlags flags)
        {
            return (Flags & flags) != 0;
        }
    }
}