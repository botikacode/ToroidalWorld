namespace ToroidalWorld.GameLogic.Entities.Definitions
{
    public sealed class IcebreakerDefinition
    {
        public bool Enabled { get; set; }

        public float CooldownSeconds { get; set; } = 0.1f;

        public int DeltaPerHit { get; set; } = 20;

        public int ForwardTiles { get; set; } = 3;

        public int HalfWidthTiles { get; set; } = 1;

        public IcebreakerProbeDefinition Probe { get; set; } = new IcebreakerProbeDefinition();

        public string BreakingLoopSfxKey { get; set; }
    }

    public sealed class IcebreakerProbeDefinition
    {
        public bool Enabled { get; set; } = true;

        public int ForwardTiles { get; set; } = 2;

        public int HalfWidthTiles { get; set; } = 1;

        public float CooldownSeconds { get; set; } = 0.03f;

        public int DeltaPerHit { get; set; } = 8;

        public float SlowdownPerSolidTile { get; set; } = 0.015f;

        public float MinSpeedMultiplier { get; set; } = 0.35f;
    }
}