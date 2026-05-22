using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.ECS.Systems;
using ToroidalWorld.GameLogic.Session;

namespace ToroidalWorld.GameLogic.Systems
{
    public sealed class StageStatsDebugRenderSystem : DrawSystem
    {
        private readonly GameSession _session;
        private readonly SpriteBatch _spriteBatch;
        private readonly SpriteFont _font;

        public StageStatsDebugRenderSystem(GameSession session, SpriteBatch spriteBatch, SpriteFont font)
        {
            _session = session;
            _spriteBatch = spriteBatch;
            _font = font;
        }

        public override void Draw(GameTime gameTime)
        {
            if (_session == null || _session.World == null)
                return;

            var stats = _session.Stats;
            var stage = stats.Stage;

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            _spriteBatch.DrawString(
                _font,
                $"Wave: {stage.WaveIndex}",
                new Vector2(16f, 50f),
                Color.White);

            _spriteBatch.DrawString(
                _font,
                $"Time: {stage.TimeSeconds:0.0}s",
                new Vector2(16f, 74f),
                Color.White);

            _spriteBatch.DrawString(
                _font,
                $"Kills: {stats.EnemiesKilled}",
                new Vector2(16f, 98f),
                Color.White);

            _spriteBatch.End();
        }
    }
}