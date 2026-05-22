using Microsoft.Xna.Framework;

namespace ToroidalWorld.GameLogic.Components
{
    public sealed class SpriteShadowComponent
    {
        public SpriteShadowComponent()
        {
        }

        public Vector2 OffsetPixels { get; set; } = new Vector2(6f, 10f);

        public float Alpha { get; set; } = 0.25f;

        public Vector2 ScaleMultiplier { get; set; } = new Vector2(1.0f, 0.75f);

        public bool UseEntityRotation { get; set; } = true;

        public Color Color { get; set; } = Color.Black;
    }
}