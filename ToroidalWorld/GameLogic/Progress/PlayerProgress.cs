using System;
using System.Collections.Generic;

namespace ToroidalWorld.GameLogic.Progress
{
    public sealed class PlayerProgress
    {
        public long TotalPoints { get; set; }

        public List<GameRecord> BestRuns { get; set; } = new List<GameRecord>();
    }

    public sealed class GameRecord
    {
        public DateTime UtcTimestamp { get; set; }

        public int Kills { get; set; }

        public float TimeSeconds { get; set; }

        public long Points { get; set; }
    }
}