using System.Collections.Generic;

namespace ToroidalWorld.GameLogic.Entities.Definitions
{
    public sealed class StageDefinition
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public float WaveDurationSeconds { get; set; } = 20f;

        public float SpawnIntervalSeconds { get; set; } = 0.5f;

        public int BaseEnemiesPerWave { get; set; } = 5;

        public StageScalingDefinition Scaling { get; set; } = new();

        public SpawnRingDefinition SpawnRing { get; set; } = new();

        public List<StageParticipantDefinition> Participants { get; set; } = new();

        public List<string> EnemyBases { get; set; } = new();
    }

    public sealed class StageScalingDefinition
    {
        public float EnemyCountMultiplier { get; set; } = 1.1f;
        public float HealthMultiplier { get; set; } = 1.15f;
        public float SpeedMultiplier { get; set; } = 1.05f;
        public float DamageMultiplier { get; set; } = 1.1f;
        public int MaxEnemiesPerWave { get; set; } = -1;
    }

    public sealed class SpawnRingDefinition
    {
        public float MinRadius { get; set; } = 700f;
        public float MaxRadius { get; set; } = 1100f;
    }

    public sealed class StageParticipantDefinition
    {
        public string EnemyName { get; set; }

        public int StartWave { get; set; } = 0;

        public int EndWave { get; set; } = -1;

        public float Weight { get; set; } = 1f;
    }
}