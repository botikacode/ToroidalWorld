namespace ToroidalWorld.GameLogic.Entities.Components
{
    public sealed class BaseStatsComponent
    {
        public BaseStatsComponent(
            float maxSpeed = 0f,
            float acceleration = 0f,
            float rotationSpeed = 0f,
            float turretTurnSpeed = 0f,
            float turretCooldownSeconds = 0f,
            float turretRangeTiles = 0f)
        {
            MaxSpeed = maxSpeed;
            Acceleration = acceleration;
            RotationSpeed = rotationSpeed;

            TurretTurnSpeed = turretTurnSpeed;
            TurretCooldownSeconds = turretCooldownSeconds;
            TurretRangeTiles = turretRangeTiles;
        }

        // Player base move stats
        public float MaxSpeed { get; }

        public float Acceleration { get; }

        public float RotationSpeed { get; }

        // Turret base stats
        public float TurretTurnSpeed { get; }

        public float TurretCooldownSeconds { get; }

        public float TurretRangeTiles { get; }
    }
}