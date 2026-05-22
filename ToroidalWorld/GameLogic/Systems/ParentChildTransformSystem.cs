using System;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using ToroidalWorld.GameLogic.Components;
using ToroidalWorld.GameLogic.Session;

namespace ToroidalWorld.GameLogic.Systems
{
    public sealed class ParentChildTransformSystem : EntityProcessingSystem
    {
        private readonly GameSession _session;

        private ComponentMapper<Transform2> _transformMapper;
        private ComponentMapper<ChildOfComponent> _childOfMapper;

        public ParentChildTransformSystem(GameSession session)
            : base(Aspect.All(typeof(Transform2), typeof(ChildOfComponent)))
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _transformMapper = mapperService.GetMapper<Transform2>();
            _childOfMapper = mapperService.GetMapper<ChildOfComponent>();
        }

        public override void Process(GameTime gameTime, int entityId)
        {
            var childTransform = _transformMapper.Get(entityId);
            var childOf = _childOfMapper.Get(entityId);

            if (childTransform == null || childOf == null)
                return;

            Transform2 parentTransform = null;

            try
            {
                var parentEntity = _session.World.GetEntity(childOf.ParentEntityId);
                parentTransform = parentEntity?.Get<Transform2>();
            }
            catch
            {
                return;
            }

            if (parentTransform == null)
                return;

            var rotatedOffset = Vector2.Transform(childOf.LocalOffset, Matrix.CreateRotationZ(parentTransform.Rotation));
            childTransform.Position = parentTransform.Position + rotatedOffset;
            childTransform.Rotation = parentTransform.Rotation + childOf.LocalRotation;
        }
    }
}