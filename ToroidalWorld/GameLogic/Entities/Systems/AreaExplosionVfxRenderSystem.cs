using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using MonoGame.Extended.Graphics;
using ToroidalWorld.GameLogic.Entities.Components;

namespace ToroidalWorld.GameLogic.Entities.Systems
{
    public sealed class AreaExplosionVfxRenderSystem : EntityDrawSystem
    {
        private const int RingTextureSize = 128;

        private readonly SpriteBatch _spriteBatch;
        private readonly OrthographicCamera _camera;
        private readonly GraphicsDevice _graphicsDevice;

        private readonly Dictionary<int, Texture2D> _ringTexturesByThickness = new Dictionary<int, Texture2D>();

        private readonly Vector2 _ringOrigin;

        private ComponentMapper<Transform2> _transformMapper;
        private ComponentMapper<LifeTimeComponent> _lifeTimeMapper;
        private ComponentMapper<AreaExplosionVfxComponent> _vfxMapper;

        public AreaExplosionVfxRenderSystem(SpriteBatch spriteBatch, OrthographicCamera camera, GraphicsDevice graphicsDevice)
            : base(Aspect.All(typeof(Transform2), typeof(LifeTimeComponent), typeof(AreaExplosionVfxComponent)))
        {
            _spriteBatch = spriteBatch ?? throw new ArgumentNullException(nameof(spriteBatch));
            _camera = camera ?? throw new ArgumentNullException(nameof(camera));

            _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));

            _ = GetOrCreateRingTexture(thickness: 8);
            _ringOrigin = new Vector2(RingTextureSize * 0.5f, RingTextureSize * 0.5f);
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _transformMapper = mapperService.GetMapper<Transform2>();
            _lifeTimeMapper = mapperService.GetMapper<LifeTimeComponent>();
            _vfxMapper = mapperService.GetMapper<AreaExplosionVfxComponent>();
        }

        public override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin(
                samplerState: SamplerState.PointClamp,
                blendState: BlendState.Additive,
                transformMatrix: _camera.GetViewMatrix());

            for (int i = 0; i < ActiveEntities.Count; i++)
            {
                int entityId = ActiveEntities[i];

                var transform = _transformMapper.Get(entityId);
                var life = _lifeTimeMapper.Get(entityId);
                var vfx = _vfxMapper.Get(entityId);

                if (transform == null || life == null || vfx == null)
                    continue;

                float duration = MathF.Max(vfx.DurationSeconds, 0.0001f);
                float t = 1f - (life.RemainingTime / duration);
                t = MathHelper.Clamp(t, 0f, 1f);

                float eased = 1f - ((1f - t) * (1f - t));

                float radius = MathF.Max(0f, vfx.MaxRadiusPixels) * eased;
                if (radius <= 0.5f)
                    continue;

                float alpha = 1f - t;
                if (alpha <= 0.01f)
                    continue;

                int thickness = (int)MathF.Round(MathF.Max(1f, vfx.ThicknessPixels));
                if (thickness > (RingTextureSize / 2))
                    thickness = RingTextureSize / 2;

                Texture2D ringTexture = GetOrCreateRingTexture(thickness);

                float diameter = radius * 2f;
                float scale = diameter / ringTexture.Width;

                _spriteBatch.Draw(
                    ringTexture,
                    position: transform.Position,
                    sourceRectangle: null,
                    color: vfx.Color * alpha,
                    rotation: 0f,
                    origin: _ringOrigin,
                    scale: scale,
                    effects: SpriteEffects.None,
                    layerDepth: 0f);
            }

            _spriteBatch.End();
        }

        private Texture2D GetOrCreateRingTexture(int thickness)
        {
            if (_ringTexturesByThickness.TryGetValue(thickness, out var tex))
                return tex;

            tex = CreateRingTexture(_graphicsDevice, size: RingTextureSize, thickness: thickness);
            _ringTexturesByThickness[thickness] = tex;
            return tex;
        }

        private static Texture2D CreateRingTexture(GraphicsDevice gd, int size, int thickness)
        {
            var tex = new Texture2D(gd, size, size);

            var data = new Color[size * size];

            float cx = (size - 1) * 0.5f;
            float cy = (size - 1) * 0.5f;
            float r = (size * 0.5f) - 1f;
            float halfTh = MathF.Max(1f, thickness * 0.5f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - cx;
                    float dy = y - cy;
                    float dist = MathF.Sqrt((dx * dx) + (dy * dy));

                    float delta = MathF.Abs(dist - r);

                    float a = 0f;
                    if (delta <= halfTh)
                    {
                        a = 1f - (delta / halfTh);
                        a = MathHelper.Clamp(a, 0f, 1f);
                    }

                    data[(y * size) + x] = new Color(1f, 1f, 1f, a);
                }
            }

            tex.SetData(data);
            return tex;
        }
    }
}