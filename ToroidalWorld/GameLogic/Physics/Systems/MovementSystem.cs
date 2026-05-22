using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using ToroidalWorld.GameLogic.Physics.Components;

namespace ToroidalWorld.GameLogic.Physics.Systems
{
    public class MovementSystem : EntityProcessingSystem
    {
        private ComponentMapper<Transform2> _transformMapper;
        private ComponentMapper<MovementIntent> _intentMapper;
        private ComponentMapper<MovementState> _stateMapper;

        public MovementSystem()
            : base(Aspect.All(typeof(Transform2), typeof(MovementIntent), typeof(MovementState)))
        { }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _transformMapper = mapperService.GetMapper<Transform2>();
            _intentMapper = mapperService.GetMapper<MovementIntent>();
            _stateMapper = mapperService.GetMapper<MovementState>();
        }

        public override void Process(GameTime gameTime, int entityId)
        {
            var transform = _transformMapper.Get(entityId);
            var intent = _intentMapper.Get(entityId);
            var state = _stateMapper.Get(entityId);

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            state.OldPosition = transform.Position;

            state.Velocity = intent.Velocity;
            state.ProposedPosition = state.OldPosition + intent.Velocity * dt;
            state.ProposedRotation = transform.Rotation + intent.RotationDelta;
        }
    }
}