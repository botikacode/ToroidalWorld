namespace ToroidalWorld.GameLogic.Entities.Definitions
{
    public sealed class TurretMountDefinition
    {
        public string Turret { get; set; }

        public float LocalOffsetX { get; set; }

        public float LocalOffsetY { get; set; }

        public float LocalRotationRadians { get; set; } = 0f;
    }
}