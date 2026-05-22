namespace ToroidalWorld.GameLogic.Entities.Components
{
    public sealed class PlayerLevelComponent
    {
        public int Level { get; set; } = 0;

        public int Experience { get; set; } = 0;

        public int ExperienceToNextLevel { get; set; } = 10;

        public int PendingLevelUps { get; set; } = 0;

        public float ExperienceMagnetRangePixels { get; set; } = 60f;
    }
}