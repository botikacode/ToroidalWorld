using Microsoft.Xna.Framework;

namespace ToroidalWorld.GameLogic.Entities.Components
{
    public sealed class TurretPickupComponent
    {
        public string TurretName { get; set; }

        public int TargetMountIndex { get; set; } = 0;

        public float SpinRadiansPerSecond { get; set; } = 6f;

        public float MarkerIntervalSeconds { get; set; } = 0.25f;

        public float MarkerRadiusPixels { get; set; } = 22f;

        public float MarkerDurationSeconds { get; set; } = 0.20f;

        public Color MarkerColor { get; set; } = Color.Yellow;

        public float MarkerThicknessPixels { get; set; } = 18f;

        public Vector2 MarkerOffsetPixels { get; set; } = Vector2.Zero;

        public float MarkerTimerSeconds { get; set; } = 0f;

        public TurretPickupComponent()
        {
        }

        public TurretPickupComponent(string turretName)
        {
            TurretName = turretName;
        }
    }
}