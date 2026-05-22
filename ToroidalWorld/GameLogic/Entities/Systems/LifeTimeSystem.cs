using Microsoft.Xna.Framework;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using ToroidalWorld.GameLogic.Entities.Components;
using ToroidalWorld.GameLogic.Session;

namespace ToroidalWorld.GameLogic.Entities.Systems
{
    public sealed class LifeTimeSystem : EntityUpdateSystem
    {
        private readonly GameSession _gameSession;

        private ComponentMapper<LifeTimeComponent> _lifeTimeMapper;

        public LifeTimeSystem(GameSession gameSession)
            : base(Aspect.All(typeof(LifeTimeComponent)))
        {
            _gameSession = gameSession;
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _lifeTimeMapper = mapperService.GetMapper<LifeTimeComponent>();
        }

        public override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            for (int i = ActiveEntities.Count - 1; i >= 0; i--)
            {
                int entityId = ActiveEntities[i];
                var lifeTime = _lifeTimeMapper.Get(entityId);
                if (lifeTime == null)
                    continue;

                lifeTime.RemainingTime -= dt;

                if (lifeTime.RemainingTime <= 0f)
                    _gameSession.DestroyEntityById(entityId);
            }
        }
    }
}