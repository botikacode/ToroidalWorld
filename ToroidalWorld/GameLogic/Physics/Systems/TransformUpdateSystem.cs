using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using ToroidalWorld.GameLogic.Components;
using ToroidalWorld.GameLogic.Physics.Components;

namespace ToroidalWorld.GameLogic.Physics.Systems
{

    public class TransformUpdateSystem : EntityProcessingSystem
    {
        private ComponentMapper<Transform2> _transformMapper;
        private ComponentMapper<MovementState> _stateMapper;

        public TransformUpdateSystem()
            : base(Aspect.All(typeof(Transform2), typeof(MovementState)))
        { }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _transformMapper = mapperService.GetMapper<Transform2>();
            _stateMapper = mapperService.GetMapper<MovementState>();
        }

        public override void Process(Microsoft.Xna.Framework.GameTime gameTime, int entityId)
        {
            var transform = _transformMapper.Get(entityId);
            var state = _stateMapper.Get(entityId);

            if (transform == null || state == null)
                return;

            transform.Position = state.ProposedPosition;
            transform.Rotation = state.ProposedRotation;
        }
    }
}
