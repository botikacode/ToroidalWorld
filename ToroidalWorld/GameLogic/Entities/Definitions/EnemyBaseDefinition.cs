using System.Collections.Generic;

namespace ToroidalWorld.GameLogic.Entities.Definitions
{
    public sealed class EnemyBaseDefinition
    {
        public string Name { get; set; }

        public string SpriteSheet { get; set; }

        public int ColliderWidth { get; set; } = 24;

        public int ColliderHeight { get; set; } = 52;

        public int Health { get; set; } = 250;

        public int ContactDamage { get; set; } = 25;

        public string DeathVfx { get; set; }

        public string TurretDrop { get; set; }

        public List<TurretMountDefinition> TurretMounts { get; set; } = new();
    }
}