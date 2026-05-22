namespace ToroidalWorld.GameLogic.Entities.Components
{
    public sealed class TurretAimComponent
    {
        public float TurnSpeed { get; set; } = 4f;
        public float AimOffsetRadians { get; set; } = 0f;
        public float RestLocalRotation { get; set; } = 0f;
        public float FireAngleToleranceRadians { get; set; } = 0.05f;
    }
}