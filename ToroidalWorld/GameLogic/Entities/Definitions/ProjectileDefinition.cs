namespace ToroidalWorld.GameLogic.Entities.Definitions
{
    public sealed class ProjectileDefinition
    {
        public string Name { get; set; }

        public string SpriteSheet { get; set; }

        public float Speed { get; set; } = 600f;

        public float RotationSpeed { get; set; } = 10f;

        public float LifetimeSeconds { get; set; } = 4f;

        public int ColliderSize { get; set; } = 1;

        public int Damage { get; set; } = 60;

        public float DamageInAreaRadius { get; set; } = 0f;

        public int DamageInAreaDamage { get; set; } = 0;

        public int TerrainExplosionRadiusInTiles { get; set; } = 0;

        public bool IsGuided { get; set; } = false;
        public float TargetSearchRangeTiles { get; set; } = 0f;
    }
}