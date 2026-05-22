using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using System;
using ToroidalWorld.GameLogic.Entities.Components;
using ToroidalWorld.GameLogic.Map;
using ToroidalWorld.GameLogic.Session;

namespace ToroidalWorld.GameLogic.Entities.Systems
{
    public sealed class ExperienceOrbMagnetSystem : EntityUpdateSystem
    {
        private readonly GameSession _session;
        private readonly MapData _map;

        private ComponentMapper<ExperienceOrbComponent> _orbMapper;
        private ComponentMapper<Transform2> _transformMapper;
        private ComponentMapper<ToroidalWorld.GameLogic.Physics.Components.MovementIntent> _intentMapper;
        private ComponentMapper<MoveStatsComponent> _moveStatsMapper;

        private const float CollectDistancePixels = 12f;

        public ExperienceOrbMagnetSystem(GameSession session, MapData map)
            : base(Aspect.All(typeof(ExperienceOrbComponent), typeof(Transform2), typeof(ToroidalWorld.GameLogic.Physics.Components.MovementIntent), typeof(MoveStatsComponent)))
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _map = map ?? throw new ArgumentNullException(nameof(map));
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _orbMapper = mapperService.GetMapper<ExperienceOrbComponent>();
            _transformMapper = mapperService.GetMapper<Transform2>();
            _intentMapper = mapperService.GetMapper<ToroidalWorld.GameLogic.Physics.Components.MovementIntent>();
            _moveStatsMapper = mapperService.GetMapper<MoveStatsComponent>();
        }

        public override void Update(GameTime gameTime)
        {
            if (!_session.HasPlayer)
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

            float expGainMult = mult?.ExperienceGainMultiplier ?? 1f;
            if (expGainMult < 0f)
                expGainMult = 0f;

            Vector2 playerPos = playerTransform.Position;

            for (int i = ActiveEntities.Count - 1; i >= 0; i--)
            {
                int entityId = ActiveEntities[i];

                var orb = _orbMapper.Get(entityId);
                var transform = _transformMapper.Get(entityId);
                var intent = _intentMapper.Get(entityId);
                var move = _moveStatsMapper.Get(entityId);

                if (orb == null || transform == null || intent == null || move == null)
                    continue;

                Vector2 orbPos = transform.Position;
                Vector2 playerPosToroidal = _map.GetToroidalPosition(playerPos, orbPos);

                Vector2 toPlayer = playerPosToroidal - orbPos;
                float distSq = toPlayer.LengthSquared();

                if (distSq <= collectDistanceSq)
                {
                    ApplyExperience(level, orb.Amount, expGainMult);

                    intent.Velocity = Vector2.Zero;
                    intent.RotationDelta = 0f;

                    _session.DestroyEntityById(entityId);
                    continue;
                }

                if (distSq <= magnetRangeSq && distSq > 0.0001f)
                {
                    toPlayer.Normalize();

                    float speed = move.MaxSpeed;
                    if (speed < 0f)
                        speed = 0f;

                    intent.Velocity = toPlayer * speed;
                    intent.RotationDelta = 0f;
                }
                else
                {
                    intent.Velocity = Vector2.Zero;
                    intent.RotationDelta = 0f;
                }
            }
        }

        private static void ApplyExperience(PlayerLevelComponent level, int rawAmount, float expGainMult)
        {
            if (level == null)
                return;

            if (rawAmount <= 0)
                return;

            int gained = (int)MathF.Round(rawAmount * expGainMult);
            if (gained <= 0)
                return;

            level.Experience += gained;

            while (level.ExperienceToNextLevel > 0 && level.Experience >= level.ExperienceToNextLevel)
            {
                level.Experience -= level.ExperienceToNextLevel;
                level.Level++;
                level.PendingLevelUps++;

                level.ExperienceToNextLevel = ComputeNextLevelThreshold(level.Level);
                if (level.ExperienceToNextLevel < 1)
                    level.ExperienceToNextLevel = 1;
            }
        }

        private static int ComputeNextLevelThreshold(int level)
        {
            if (level < 0)
                level = 0;

            const float baseXp = 10f;
            const float growth = 1.35f;

            float value = baseXp * MathF.Pow(growth, level);
            return (int)MathF.Round(value);
        }
    }
}