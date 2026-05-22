using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using MonoGame.Extended.Graphics;
using ToroidalWorld.GameLogic.Components;

namespace ToroidalWorld.GameLogic.Systems
{
    public sealed class SpriteShadowRenderSystem : EntityDrawSystem
    {
        private readonly SpriteBatch _spriteBatch;
        private readonly OrthographicCamera _camera;

        private ComponentMapper<AnimatedSprite> _animatedSpriteMapper;
        private ComponentMapper<Transform2> _transformMapper;
        private ComponentMapper<SpriteShadowComponent> _shadowMapper;

        public SpriteShadowRenderSystem(SpriteBatch spriteBatch, OrthographicCamera camera)
            : base(Aspect.All(typeof(AnimatedSprite), typeof(Transform2), typeof(SpriteShadowComponent)))
        {
            _spriteBatch = spriteBatch;
            _camera = camera;
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _animatedSpriteMapper = mapperService.GetMapper<AnimatedSprite>();
            _transformMapper = mapperService.GetMapper<Transform2>();
            _shadowMapper = mapperService.GetMapper<SpriteShadowComponent>();
        }

        public override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: _camera.GetViewMatrix());

            foreach (var entityId in ActiveEntities)
            {
                var animatedSprite = _animatedSpriteMapper.Get(entityId);
                var transform = _transformMapper.Get(entityId);
                var shadow = _shadowMapper.Get(entityId);

                if (animatedSprite == null || transform == null || shadow == null)
                    continue;

                float alpha = MathHelper.Clamp(shadow.Alpha, 0f, 1f);
                if (alpha <= 0.001f)
                    continue;

                var previousColor = animatedSprite.Color;

                try
                {
                    animatedSprite.Color = shadow.Color * alpha;

                    Vector2 position = transform.Position + shadow.OffsetPixels;
                    float rotation = shadow.UseEntityRotation ? transform.Rotation : 0f;
                    Vector2 scale = transform.Scale * shadow.ScaleMultiplier;

                    _spriteBatch.Draw(animatedSprite, position, rotation, scale);
                }
                finally
                {
                    animatedSprite.Color = previousColor;
                }
            }

            _spriteBatch.End();
        }
    }
}