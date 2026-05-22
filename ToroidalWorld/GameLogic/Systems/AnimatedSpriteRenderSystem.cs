using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using MonoGame.Extended.Graphics;
using ToroidalWorld.GameLogic.Components;

namespace ToroidalWorld.GameLogic.Systems
{
    public class AnimatedSpriteRenderSystem
    : EntityDrawSystem
    {
        private SpriteBatch _spriteBatch;
        private OrthographicCamera _camera;
        private ComponentMapper<AnimatedSprite> _animatedSpriteMapper;
        private ComponentMapper<Transform2> _transformMapper;
        private ComponentMapper<ChildOfComponent> _childOfMapper;

        public AnimatedSpriteRenderSystem(SpriteBatch spriteBatch, OrthographicCamera camera)
            : base(Aspect.All(typeof(AnimatedSprite), typeof(Transform2)))
        {
            _spriteBatch = spriteBatch;
            _camera = camera;
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _animatedSpriteMapper = mapperService.GetMapper<AnimatedSprite>();
            _transformMapper = mapperService.GetMapper<Transform2>();
            _childOfMapper = mapperService.GetMapper<ChildOfComponent>();
        }

        public override void Draw(GameTime gameTime)
        {
            Matrix transformMatrix = _camera.GetViewMatrix();

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: transformMatrix);

            foreach (var entityId in ActiveEntities)
            {
                if (_childOfMapper.Get(entityId) != null)
                    continue;

                var animatedSprite = _animatedSpriteMapper.Get(entityId);
                var transform = _transformMapper.Get(entityId);

                _spriteBatch.Draw(animatedSprite, transform.Position, transform.Rotation, transform.Scale);
            }

            foreach (var entityId in ActiveEntities)
            {
                if (_childOfMapper.Get(entityId) == null)
                    continue;

                var animatedSprite = _animatedSpriteMapper.Get(entityId);
                var transform = _transformMapper.Get(entityId);

                _spriteBatch.Draw(animatedSprite, transform.Position, transform.Rotation, transform.Scale);
            }

            _spriteBatch.End();
        }
    }
}