using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using System;
using ToroidalWorld.GameLogic.Entities.Components;

namespace ToroidalWorld.GameLogic.Entities.Systems
{
    public sealed class TurretPickupSpinSystem : EntityUpdateSystem
    {
        private ComponentMapper<TurretPickupComponent> _pickupMapper;
        private ComponentMapper<Transform2> _transformMapper;

        public TurretPickupSpinSystem()
            : base(Aspect.All(typeof(TurretPickupComponent), typeof(Transform2)))
        {
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _pickupMapper = mapperService.GetMapper<TurretPickupComponent>();
            _transformMapper = mapperService.GetMapper<Transform2>();
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            for (int i = 0; i < ActiveEntities.Count; i++)
            {
                int entityId = ActiveEntities[i];

                var pickup = _pickupMapper.Get(entityId);
                var transform = _transformMapper.Get(entityId);

                if (pickup == null || transform == null)
                    continue;

                transform.Rotation += pickup.SpinRadiansPerSecond * dt;
            }
        }
    }
}