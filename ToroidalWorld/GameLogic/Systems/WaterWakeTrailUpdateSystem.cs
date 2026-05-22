using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using System;
using ToroidalWorld.GameLogic.Components;

namespace ToroidalWorld.GameLogic.Systems
{
    public sealed class WaterWakeTrailUpdateSystem : EntityUpdateSystem
    {
        private ComponentMapper<Transform2> _transformMapper;
        private ComponentMapper<WaterWakeTrailComponent> _wakeMapper;

        public WaterWakeTrailUpdateSystem()
            : base(Aspect.All(typeof(Transform2), typeof(WaterWakeTrailComponent)))
        {
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _transformMapper = mapperService.GetMapper<Transform2>();
            _wakeMapper = mapperService.GetMapper<WaterWakeTrailComponent>();
        }

        public override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (dt <= 0f)
                return;

            for (int e = 0; e < ActiveEntities.Count; e++)
            {
                int entityId = ActiveEntities[e];

                var transform = _transformMapper.Get(entityId);
                var wake = _wakeMapper.Get(entityId);

                if (transform == null || wake == null)
                    continue;

                TickAndCull(wake, dt);
                TrySpawnAlongMovement(wake, transform.Position);
            }
        }

        private static void TickAndCull(WaterWakeTrailComponent wake, float dt)
        {
            float life = MathHelper.Max(0.0001f, wake.LifeSeconds);

            for (int i = 0; i < wake.Stamps.Count; i++)
            {
                var s = wake.Stamps[i];
                s.AgeSeconds += dt;
                wake.Stamps[i] = s;
            }

            for (int i = wake.Stamps.Count - 1; i >= 0; i--)
            {
                if (wake.Stamps[i].AgeSeconds >= life)
                    wake.Stamps.RemoveAt(i);
            }
        }

        private static void TrySpawnAlongMovement(WaterWakeTrailComponent wake, Vector2 currentPosition)
        {
            if (!wake.HasLastStampPosition)
            {
                wake.HasLastStampPosition = true;
                wake.LastStampPosition = currentPosition;
                return;
            }

            Vector2 delta = currentPosition - wake.LastStampPosition;
            float dist = delta.Length();

            float minDist = MathHelper.Max(0.01f, wake.MinDistancePixels);
            if (dist < minDist)
                return;

            Vector2 dir = delta / dist;

            int steps = (int)(dist / minDist);
            if (steps < 1)
                return;

            if (steps > 24)
                steps = 24;

            float baseRotation = MathF.Atan2(dir.Y, dir.X);

            for (int i = 1; i <= steps; i++)
            {
                Vector2 p = wake.LastStampPosition + (dir * (i * minDist));

                wake.Stamps.Add(new WaterWakeTrailComponent.Stamp
                {
                    Position = p - (dir * wake.BehindOffsetPixels),
                    Rotation = baseRotation,
                    AgeSeconds = 0f
                });
            }

            while (wake.Stamps.Count > wake.MaxStamps && wake.Stamps.Count > 0)
                wake.Stamps.RemoveAt(0);

            wake.LastStampPosition += dir * (steps * minDist);
        }
    }
}