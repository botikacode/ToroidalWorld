using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using System;
using ToroidalWorld.GameLogic.Entities.Components;
using ToroidalWorld.GameLogic.Map;
using ToroidalWorld.GameLogic.Physics.Components;
using ToroidalWorld.GameLogic.Session;

namespace ToroidalWorld.GameLogic.Entities.Systems
{
    public sealed class TurretPickupMagnetSystem : EntityUpdateSystem
    {
        private readonly GameSession _session;
        private readonly MapData _map;

        private ComponentMapper<TurretPickupComponent> _pickupMapper;
        private ComponentMapper<Transform2> _transformMapper;
        private ComponentMapper<MovementIntent> _intentMapper;
        private ComponentMapper<MoveStatsComponent> _moveStatsMapper;

        private const float CollectDistancePixels = 12f;

        public TurretPickupMagnetSystem(GameSession session, MapData map)
            : base(Aspect.All(typeof(TurretPickupComponent), typeof(Transform2), typeof(MovementIntent), typeof(MoveStatsComponent)))
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _map = map ?? throw new ArgumentNullException(nameof(map));
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _pickupMapper = mapperService.GetMapper<TurretPickupComponent>();
            _transformMapper = mapperService.GetMapper<Transform2>();
            _intentMapper = mapperService.GetMapper<MovementIntent>();
            _moveStatsMapper = mapperService.GetMapper<MoveStatsComponent>();
        }

        public override void Update(GameTime gameTime)
        {
            if (!_session.HasPlayer)
                return;

            // Si ya hay una selección pendiente, no queremos re-disparar ni atraer más pickups.
            if (_session.HasPendingTurretPickup)
                return;

            Transform2 playerTransform = null;
            Entity player = null;

            try
            {
                player = _session.World.GetEntity(_session.PlayerEntityId);
                playerTransform = player?.Get<Transform2>();
            }
            catch
            {
                return;
            }

            if (playerTransform == null || player == null)
                return;

            var level = player.Get<PlayerLevelComponent>();
            var mult = player.Get<StatMultipliersComponent>();

            float baseMagnetRange = level?.ExperienceMagnetRangePixels ?? 200f;

            float rangeMult = mult?.ExperienceMagnetRangeMultiplier ?? 1f;
            if (rangeMult < 0f)
                rangeMult = 0f;

            float magnetRange = baseMagnetRange * rangeMult;
            if (magnetRange < 0f)
                magnetRange = 0f;

            float magnetRangeSq = magnetRange * magnetRange;
            float collectDistanceSq = CollectDistancePixels * CollectDistancePixels;

            Vector2 playerPos = playerTransform.Position;

            for (int i = ActiveEntities.Count - 1; i >= 0; i--)
            {
                int entityId = ActiveEntities[i];

                var pickup = _pickupMapper.Get(entityId);
                var transform = _transformMapper.Get(entityId);
                var intent = _intentMapper.Get(entityId);
                var move = _moveStatsMapper.Get(entityId);

                if (pickup == null || transform == null || intent == null || move == null)
                    continue;

                intent.RotationDelta = 0f;

                Vector2 pickupPos = transform.Position;
                Vector2 playerPosToroidal = _map.GetToroidalPosition(playerPos, pickupPos);

                Vector2 toPlayer = playerPosToroidal - pickupPos;
                float distSq = toPlayer.LengthSquared();

                if (distSq <= collectDistanceSq)
                {
                    if (_session.TryBeginTurretPickupSelection(entityId, pickup.TurretName))
                    {
                        intent.Velocity = Vector2.Zero;
                        return;
                    }

                    continue;
                }

                if (distSq <= magnetRangeSq && distSq > 0.0001f)
                {
                    toPlayer.Normalize();

                    float speed = move.MaxSpeed;
                    if (speed < 0f)
                        speed = 0f;

                    intent.Velocity = toPlayer * speed;
                }
                else
                {
                    intent.Velocity = Vector2.Zero;
                }
            }
        }
    }
}