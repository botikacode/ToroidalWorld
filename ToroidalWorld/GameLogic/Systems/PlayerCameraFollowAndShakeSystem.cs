using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using System;
using ToroidalWorld.GameLogic.Entities.Components;
using ToroidalWorld.GameLogic.Session;

namespace ToroidalWorld.GameLogic.Systems
{
    public sealed class PlayerCameraFollowAndShakeSystem : UpdateSystem
    {
        private static readonly Random _rng = new Random();

        private readonly GameSession _session;

        private int _lastHealth = -1;

        private float _shakeRemainingSeconds;
        private Vector2 _shakeOffset;

        public float ShakeDurationSeconds { get; set; } = 0.18f;

        public float ShakeMagnitudePixels { get; set; } = 6f;

        public PlayerCameraFollowAndShakeSystem(GameSession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public override void Update(GameTime gameTime)
        {
            if (_session.World == null || !_session.HasPlayer)
                return;

            Entity player;

            try { player = _session.World.GetEntity(_session.PlayerEntityId); }
            catch { return; }

            if (player == null)
                return;

            var transform = player.Get<Transform2>();
            var health = player.Get<HealthComponent>();

            if (transform == null)
                return;

            DetectDamage(health);
            TickShake((float)gameTime.ElapsedGameTime.TotalSeconds);

            _session.Camera.LookAt(transform.Position + _shakeOffset);
        }

        private void DetectDamage(HealthComponent health)
        {
            if (health == null)
                return;

            if (_lastHealth < 0)
            {
                _lastHealth = health.CurrentHealth;
                return;
            }

            if (health.CurrentHealth < _lastHealth)
                _shakeRemainingSeconds = ShakeDurationSeconds;

            _lastHealth = health.CurrentHealth;
        }

        private void TickShake(float dt)
        {
            _shakeOffset = Vector2.Zero;

            if (_shakeRemainingSeconds <= 0f)
                return;

            _shakeRemainingSeconds -= dt;
            if (_shakeRemainingSeconds < 0f)
                _shakeRemainingSeconds = 0f;

            float t = ShakeDurationSeconds <= 0f ? 0f : (_shakeRemainingSeconds / ShakeDurationSeconds);
            float mag = ShakeMagnitudePixels * t;

            _shakeOffset = new Vector2(NextFloat(-1f, 1f), NextFloat(-1f, 1f)) * mag;
        }

        private static float NextFloat(float min, float max)
        {
            return (float)(_rng.NextDouble() * (max - min) + min);
        }
    }
}