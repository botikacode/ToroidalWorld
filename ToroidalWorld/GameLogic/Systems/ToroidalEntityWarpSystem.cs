using System;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using ToroidalWorld.GameLogic.Map;
using ToroidalWorld.GameLogic.Session;

namespace ToroidalWorld.GameLogic.Systems
{
    public sealed class ToroidalEntityWarpSystem : EntityUpdateSystem
    {
        private readonly GameSession _session;
        private readonly MapData _map;

        private ComponentMapper<Transform2> _transformMapper;

        public ToroidalEntityWarpSystem(GameSession session, MapData map)
            : base(Aspect.All(typeof(Transform2)))
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _map = map ?? throw new ArgumentNullException(nameof(map));
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _transformMapper = mapperService.GetMapper<Transform2>();
        }

        public override void Update(GameTime gameTime)
        {
            if (!_session.TryGetPlayerTransform(out var playerTransform) || playerTransform == null)
                return;

            Vector2 referencePos = playerTransform.Position;

            for (int i = 0; i < ActiveEntities.Count; i++)
            {
                int entityId = ActiveEntities[i];

                var transform = _transformMapper.Get(entityId);
                if (transform == null)
                    continue;

                transform.Position = _map.GetToroidalPosition(transform.Position, referencePos);
            }
        }
    }
}