using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.ECS.Systems;
using ToroidalWorld.GameLogic.Session;

namespace ToroidalWorld.GameLogic.Systems
{
    public sealed class WorldLoadingOverlayRenderSystem : DrawSystem
    {
        private readonly GameSession _session;
        private readonly SpriteBatch _spriteBatch;
        private readonly SpriteFont _font;

        private Texture2D _pixel;

        public WorldLoadingOverlayRenderSystem(GameSession session, SpriteBatch spriteBatch, SpriteFont font)
        {
            _session = session;
            _spriteBatch = spriteBatch;
            _font = font;
        }

        public override void Draw(GameTime gameTime)
        {
            if (_session == null || _session.IsWorldReady)
                return;

            _pixel ??= CreatePixel();

            int ready = _session.WorldReadyChunks;
            int total = _session.WorldTotalChunks;
            float percent = total <= 0 ? 0f : (ready * 100f / total);

            var gd = _spriteBatch.GraphicsDevice;

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            _spriteBatch.Draw(
                _pixel,
                new Rectangle(0, 0, gd.Viewport.Width, gd.Viewport.Height),
                Color.Black);

            _spriteBatch.DrawString(
                _font,
                $"Loading... {percent:0}% ({ready}/{total})",
                new Vector2(24f, 24f),
                Color.White);

            _spriteBatch.End();
        }

        private Texture2D CreatePixel()
        {
            var tex = new Texture2D(_spriteBatch.GraphicsDevice, 1, 1);
            tex.SetData(new[] { Color.White });
            return tex;
        }
    }
}