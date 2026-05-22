using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.ECS.Systems;
using ToroidalWorld.GameLogic.Entities.Components;
using ToroidalWorld.GameLogic.Session;

namespace ToroidalWorld.GameLogic.Systems
{
    public sealed class PlayerHealthDebugRenderSystem : DrawSystem
    {
        private readonly GameSession _session;
        private readonly SpriteBatch _spriteBatch;
        private readonly SpriteFont _font;

        public PlayerHealthDebugRenderSystem(GameSession session, SpriteBatch spriteBatch, SpriteFont font)
        {
            _session = session;
            _spriteBatch = spriteBatch;
            _font = font;
        }

        public override void Draw(GameTime gameTime)
        {
            if (_session == null || _session.World == null)
                return;

            if (!_session.HasPlayer)
                return;

            HealthComponent health = null;

            try
            {
                var player = _session.World.GetEntity(_session.PlayerEntityId);
                health = player?.Get<HealthComponent>();
            }
            catch
            {
                return;
            }

            if (health == null)
                return;

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            _spriteBatch.DrawString(
                _font,
                $"HP: {health.CurrentHealth}/{health.MaxHealth}",
                new Vector2(16f, 30f),
                Color.White);

            _spriteBatch.End();
        }
    }
}