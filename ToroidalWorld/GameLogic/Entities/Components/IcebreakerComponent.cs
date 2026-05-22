namespace ToroidalWorld.GameLogic.Entities.Components
{
    public sealed class IcebreakerComponent
    {
        // Post-collision: damage (breaking) + cooldown (recovery)
        public float CooldownSeconds { get; set; } = 0.1f;

        public float RemainingSeconds { get; set; }

        public int DeltaPerHit { get; set; } = 20;

        public int ForwardTiles { get; set; } = 3;

        public int HalfWidthTiles { get; set; } = 1;

        // Pre-collision: probe (checking if breaking would happen, without applying damage or cooldown)
        public bool ProbeEnabled { get; set; } = true;

        public int ProbeForwardTiles { get; set; } = 2;

        public int ProbeHalfWidthTiles { get; set; } = 1;

        public float ProbeCooldownSeconds { get; set; } = 0.03f;

        public float ProbeRemainingSeconds { get; set; }

        public int ProbeDeltaPerHit { get; set; } = 8;

        public float SlowdownPerSolidTile { get; set; } = 0.015f;

        public float MinSpeedMultiplier { get; set; } = 0.35f;

        // Sound effects
        public string BreakingLoopSfxKey { get; set; }
    }
}