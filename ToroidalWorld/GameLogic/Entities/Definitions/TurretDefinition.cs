namespace ToroidalWorld.GameLogic.Entities.Definitions
{
    public sealed class TurretDefinition
    {
        public string Name { get; set; }

        public string Description { get; set; } = string.Empty;

        public string SpriteSheet { get; set; }

        public float TurnSpeed { get; set; } = 4f;

        public float AimOffsetRadians { get; set; } = 0f;

        public float FireAngleToleranceRadians { get; set; } = 0.05f;

        public float TargetSearchRangeTiles { get; set; } = 80f;

        public float CooldownSeconds { get; set; } = 0.5f;

        public string Projectile { get; set; }

        public string ShootSound { get; set; }
    }
}