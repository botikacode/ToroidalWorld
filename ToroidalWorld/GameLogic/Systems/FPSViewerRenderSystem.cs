using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;

namespace ToroidalWorld.GameLogic.Systems
{
    public class FPSViewerRenderSystem : EntityDrawSystem
    {
        private readonly SpriteBatch _spriteBatch;
        private readonly SpriteFont _font;

        public override void Initialize(IComponentMapperService mapperService)
        {

        }

        public FPSViewerRenderSystem(SpriteBatch spriteBatch, SpriteFont font)
            : base(Aspect.All())
        {
            _spriteBatch = spriteBatch;
            _font = font;
        }

        public override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin();

            var delta = Math.Max(gameTime.ElapsedGameTime.TotalSeconds, 1e-6);
            var fps = (int)(1.0 / delta);
            var fpsText = $"FPS: {fps}";

            _spriteBatch.DrawString(_font, fpsText, new Vector2(10, 10), Color.White);

            _spriteBatch.End();
        }
    }
}