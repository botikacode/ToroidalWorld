namespace ToroidalWorld.GameLogic.Session
{
    public sealed class StageRuntimeState
    {
        public string StageId { get; set; }

        public int WaveIndex { get; set; }

        public float TimeSeconds { get; set; }

        public float WaveTimeSeconds { get; set; }

        public int TargetEnemiesThisWave { get; set; }

        public int SpawnedEnemiesThisWave { get; set; }
    }
}