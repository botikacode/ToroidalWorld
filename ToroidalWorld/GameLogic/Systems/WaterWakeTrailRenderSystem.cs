using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using ToroidalWorld.GameLogic.Components;

namespace ToroidalWorld.GameLogic.Systems
{
    public sealed class WaterWakeTrailRenderSystem : EntityDrawSystem
    {
        private readonly SpriteBatch _spriteBatch;
        private readonly OrthographicCamera _camera;

        private readonly Texture2D _wakeTexture;
        private readonly Vector2 _wakeOrigin;

        private ComponentMapper<WaterWakeTrailComponent> _wakeMapper;

        public WaterWakeTrailRenderSystem(SpriteBatch spriteBatch, OrthographicCamera camera, GraphicsDevice graphicsDevice)
            : base(Aspect.All(typeof(WaterWakeTrailComponent)))
        {
            _spriteBatch = spriteBatch ?? throw new ArgumentNullException(nameof(spriteBatch));
            _camera = camera ?? throw new ArgumentNullException(nameof(camera));

            if (graphicsDevice == null)
                throw new ArgumentNullException(nameof(graphicsDevice));

            _wakeTexture = CreateWakeTexture(graphicsDevice, width: 64, height: 32);
            _wakeOrigin = new Vector2(_wakeTexture.Width * 0.5f, _wakeTexture.Height * 0.5f);
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _wakeMapper = mapperService.GetMapper<WaterWakeTrailComponent>();
        }

        public override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin(
                samplerState: SamplerState.PointClamp,
                blendState: BlendState.AlphaBlend,
                transformMatrix: _camera.GetViewMatrix());

            for (int e = 0; e < ActiveEntities.Count; e++)
            {
                int entityId = ActiveEntities[e];
                var wake = _wakeMapper.Get(entityId);

                if (wake == null || wake.Stamps.Count == 0)
                    continue;

                float life = MathHelper.Max(0.0001f, wake.LifeSeconds);

                for (int i = 0; i < wake.Stamps.Count; i++)
                {
                    var stamp = wake.Stamps[i];

                    float t = stamp.AgeSeconds / life;
                    t = MathHelper.Clamp(t, 0f, 1f);

                    float alpha = MathHelper.Lerp(wake.StartAlpha, wake.EndAlpha, t);
                    if (alpha <= 0.001f)
                        continue;

                    Vector2 scale = Vector2.Lerp(wake.StartScale, wake.EndScale, t);

                    _spriteBatch.Draw(
                        _wakeTexture,
                        position: stamp.Position,
                        sourceRectangle: null,
                        color: wake.Color * alpha,
                        rotation: stamp.Rotation,
                        origin: _wakeOrigin,
                        scale: scale,
                        effects: SpriteEffects.None,
                        layerDepth: 0f);
                }
            }

            _spriteBatch.End();
        }

        private static Texture2D CreateWakeTexture(GraphicsDevice gd, int width, int height)
        {
            var tex = new Texture2D(gd, width, height);

            var data = new Color[width * height];

            float cx = (width - 1) * 0.5f;
            float cy = (height - 1) * 0.5f;

            float a = MathF.Max(1f, width * 0.5f);
            float b = MathF.Max(1f, height * 0.5f);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float nx = (x - cx) / a;
                    float ny = (y - cy) / b;

                    float d = (nx * nx) + (ny * ny);

                    float alpha = 0f;

                    if (d <= 1f)
                    {
                        float k = 1f - MathF.Sqrt(d);
                        alpha = k * k;
                    }

                    data[(y * width) + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            tex.SetData(data);
            return tex;
        }
    }
}