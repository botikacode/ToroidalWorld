using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.ECS.Systems;
using ToroidalWorld.GameLogic.Physics.Components;
using ToroidalWorld.GameLogic.Session;

namespace ToroidalWorld.GameLogic.Physics.Systems
{
    public sealed class CollisionDebugSystem : DrawSystem
    {
        private readonly GameSession _session;
        private readonly SpriteBatch _spriteBatch;
        private readonly OrthographicCamera _camera;
        private readonly Texture2D _pixel;

        public CollisionDebugSystem(GameSession session, SpriteBatch spriteBatch, OrthographicCamera camera, GraphicsDevice graphicsDevice)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _spriteBatch = spriteBatch ?? throw new ArgumentNullException(nameof(spriteBatch));
            _camera = camera ?? throw new ArgumentNullException(nameof(camera));

            if (graphicsDevice == null)
                throw new ArgumentNullException(nameof(graphicsDevice));

            _pixel = new Texture2D(graphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
        }

        public override void Draw(GameTime gameTime)
        {
            if (_session.World == null || !_session.HasPlayer)
                return;

            Transform2 transform = null;
            TerrainCollider terrainCollider = null;
            EntityCollider entityCollider = null;

            try
            {
                var player = _session.World.GetEntity(_session.PlayerEntityId);
                transform = player?.Get<Transform2>();
                terrainCollider = player?.Get<TerrainCollider>();
                entityCollider = player?.Get<EntityCollider>();
            }
            catch
            {
                return;
            }

            if (transform == null)
                return;

            _spriteBatch.Begin(
                samplerState: SamplerState.PointClamp,
                transformMatrix: _camera.GetViewMatrix());

            // Terrain collider (green)
            if (terrainCollider != null)
                DrawBox(
                    center: transform.Position,
                    rotation: transform.Rotation,
                    width: terrainCollider.Width,
                    height: terrainCollider.Height,
                    rotates: terrainCollider.Rotates,
                    color: Color.LimeGreen,
                    thickness: 2f);

            // Entity collider (red)
            if (entityCollider != null)
                DrawBox(
                    center: transform.Position,
                    rotation: transform.Rotation,
                    width: entityCollider.Width,
                    height: entityCollider.Height,
                    rotates: entityCollider.Rotates,
                    color: Color.Red,
                    thickness: 2f);

            DrawCross(transform.Position, size: 6f, Color.White, thickness: 2f);

            _spriteBatch.End();
        }

        private void DrawBox(Vector2 center, float rotation, int width, int height, bool rotates, Color color, float thickness)
        {
            float halfW = width * 0.5f;
            float halfH = height * 0.5f;

            Vector2 right;
            Vector2 up;

            if (rotates)
            {
                GetAxes(rotation, out right, out up);
            }
            else
            {
                right = Vector2.UnitX;
                up = Vector2.UnitY;
            }

            var p0 = center + (right * halfW) + (up * halfH);
            var p1 = center + (right * halfW) - (up * halfH);
            var p2 = center - (right * halfW) - (up * halfH);
            var p3 = center - (right * halfW) + (up * halfH);

            DrawLine(p0, p1, color, thickness);
            DrawLine(p1, p2, color, thickness);
            DrawLine(p2, p3, color, thickness);
            DrawLine(p3, p0, color, thickness);
        }

        private void DrawCross(Vector2 center, float size, Color color, float thickness)
        {
            DrawLine(center + new Vector2(-size, 0f), center + new Vector2(size, 0f), color, thickness);
            DrawLine(center + new Vector2(0f, -size), center + new Vector2(0f, size), color, thickness);
        }

        private void DrawLine(Vector2 a, Vector2 b, Color color, float thickness)
        {
            var delta = b - a;
            float length = delta.Length();
            if (length <= 0.0001f)
                return;

            float angle = (float)Math.Atan2(delta.Y, delta.X);

            _spriteBatch.Draw(
                _pixel,
                position: a,
                sourceRectangle: null,
                color: color,
                rotation: angle,
                origin: Vector2.Zero,
                scale: new Vector2(length, thickness),
                effects: SpriteEffects.None,
                layerDepth: 0f);
        }

        private static void GetAxes(float rotation, out Vector2 right, out Vector2 up)
        {
            up = new Vector2(
                (float)Math.Cos(rotation - MathHelper.PiOver2),
                (float)Math.Sin(rotation - MathHelper.PiOver2));

            right = new Vector2(-up.Y, up.X);
        }
    }
}