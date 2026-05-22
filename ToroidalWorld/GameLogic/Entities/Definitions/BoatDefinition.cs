using System.Collections.Generic;

namespace ToroidalWorld.GameLogic.Entities.Definitions
{
    public sealed class BoatDefinition
    {
        public string Name { get; set; }

        public string SpriteSheet { get; set; }

        public long RequiredPoints { get; set; }

        public int ColliderWidth { get; set; } = 42;

        public int ColliderHeight { get; set; } = 90;

        public float MaxSpeed { get; set; } = 300f;

        public float Acceleration { get; set; } = 200f;

        public float RotationSpeed { get; set; } = 2f;

        public int Health { get; set; } = 100;

        public List<TurretMountDefinition> TurretMounts { get; set; } = new();

        public IcebreakerDefinition Icebreaker { get; set; }
    }
}