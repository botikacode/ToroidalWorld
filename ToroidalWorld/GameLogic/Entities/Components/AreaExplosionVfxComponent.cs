using Microsoft.Xna.Framework;

namespace ToroidalWorld.GameLogic.Entities.Components
{
    public sealed class AreaExplosionVfxComponent
    {
        public float MaxRadiusPixels { get; set; }

        public float DurationSeconds { get; set; } = 0.10f;

        public Color Color { get; set; } = Color.Orange;

        public float ThicknessPixels { get; set; } = 8f;
    }
}