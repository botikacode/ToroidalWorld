using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using System;
using ToroidalWorld.GameLogic.Entities.Components;
using ToroidalWorld.GameLogic.Session;

namespace ToroidalWorld.GameLogic.Entities.Systems
{
    public sealed class TurretPickupMarkerVfxSystem : EntityUpdateSystem
    {
        private readonly GameSession _session;

        private ComponentMapper<TurretPickupComponent> _pickupMapper;
        private ComponentMapper<Transform2> _transformMapper;

        public TurretPickupMarkerVfxSystem(GameSession session)
            : base(Aspect.All(typeof(TurretPickupComponent), typeof(Transform2)))
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _pickupMapper = mapperService.GetMapper<TurretPickupComponent>();
            _transformMapper = mapperService.GetMapper<Transform2>();
        }

        public override void Update(GameTime gameTime)
        {
            if (_session.World == null)
                return;

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            for (int i = 0; i < ActiveEntities.Count; i++)
            {
                int entityId = ActiveEntities[i];

                var pickup = _pickupMapper.Get(entityId);
                var transform = _transformMapper.Get(entityId);

                if (pickup == null || transform == null)
                    continue;

                pickup.MarkerTimerSeconds -= dt;

                if (pickup.MarkerTimerSeconds > 0f)
                    continue;

                pickup.MarkerTimerSeconds = MathF.Max(0.01f, pickup.MarkerIntervalSeconds);

                // Centrado exactamente en el mismo punto sobre el que rota la torreta.
                Vector2 pos = transform.Position + pickup.MarkerOffsetPixels;

                EntityFactory.CreateAreaExplosionVfx(
                    _session.World,
                    position: pos,
                    radiusPixels: pickup.MarkerRadiusPixels,
                    durationSeconds: pickup.MarkerDurationSeconds,
                    color: pickup.MarkerColor,
                    thicknessPixels: pickup.MarkerThicknessPixels);
            }
        }
    }
}