namespace ToroidalWorld.GameLogic.Session
{
    public sealed class GameSessionStats
    {
        public StageRuntimeState Stage { get; } = new StageRuntimeState();

        public int EnemiesKilled { get; set; }

        public void Reset()
        {
            Stage.StageId = null;
            Stage.WaveIndex = 0;
            Stage.TimeSeconds = 0f;
            Stage.WaveTimeSeconds = 0f;
            Stage.TargetEnemiesThisWave = 0;
            Stage.SpawnedEnemiesThisWave = 0;

            EnemiesKilled = 0;
        }

        public void RegisterEnemyKilled()
        {
            EnemiesKilled++;
        }
    }
}