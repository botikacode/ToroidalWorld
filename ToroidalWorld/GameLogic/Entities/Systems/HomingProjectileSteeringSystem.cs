using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using System;
using ToroidalWorld.GameLogic.Entities.Components;
using ToroidalWorld.GameLogic.Physics.Components;
using ToroidalWorld.GameLogic.Session;

namespace ToroidalWorld.GameLogic.Entities.Systems
{
    public sealed class HomingProjectileSteeringSystem : EntityUpdateSystem
    {
        private readonly GameSession _gameSession;

        private ComponentMapper<EntityFlagsComponent> _flagsMapper;
        private ComponentMapper<MoveStatsComponent> _moveStatsMapper;
        private ComponentMapper<MovementIntent> _intentMapper;
        private ComponentMapper<Transform2> _transformMapper;
        private ComponentMapper<TargetingComponent> _targetingMapper;

        private const float RotationDeadzone = 0.01f;

        public HomingProjectileSteeringSystem(GameSession gameSession)
            : base(Aspect.All(typeof(EntityFlagsComponent), typeof(MoveStatsComponent), typeof(MovementIntent), typeof(Transform2), typeof(TargetingComponent)))
        {
            _gameSession = gameSession ?? throw new ArgumentNullException(nameof(gameSession));
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _flagsMapper = mapperService.GetMapper<EntityFlagsComponent>();
            _moveStatsMapper = mapperService.GetMapper<MoveStatsComponent>();
            _intentMapper = mapperService.GetMapper<MovementIntent>();
            _transformMapper = mapperService.GetMapper<Transform2>();
            _targetingMapper = mapperService.GetMapper<TargetingComponent>();
        }

        public override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            foreach (var entityId in ActiveEntities)
            {
                var flags = _flagsMapper.Get(entityId);
                if (flags == null || !flags.Has(EntityFlags.Projectile))
                    continue;

                var moveStats = _moveStatsMapper.Get(entityId);
                var intent = _intentMapper.Get(entityId);
                var transform = _transformMapper.Get(entityId);
                var targeting = _targetingMapper.Get(entityId);

                if (moveStats == null || intent == null || transform == null || targeting == null)
                    continue;

                float rotation = transform.Rotation;

                Vector2 forward = new Vector2(
                    (float)Math.Cos(rotation - MathHelper.PiOver2),
                    (float)Math.Sin(rotation - MathHelper.PiOver2));

                if (targeting.TargetEntityId < 0)
                {
                    intent.Velocity = forward * moveStats.MaxSpeed;
                    intent.RotationDelta = 0f;
                    continue;
                }

                Vector2 targetPosition;
                try
                {
                    var targetEntity = _gameSession.World.GetEntity(targeting.TargetEntityId);
                    var targetTransform = targetEntity?.Get<Transform2>();
                    if (targetTransform == null)
                    {
                        intent.Velocity = forward * moveStats.MaxSpeed;
                        intent.RotationDelta = 0f;
                        continue;
                    }

                    targetPosition = targetTransform.Position;
                }
                catch
                {
                    intent.Velocity = forward * moveStats.MaxSpeed;
                    intent.RotationDelta = 0f;
                    continue;
                }

                Vector2 toTarget = targetPosition - transform.Position;
                if (toTarget.LengthSquared() < 0.0001f)
                {
                    intent.Velocity = forward * moveStats.MaxSpeed;
                    intent.RotationDelta = 0f;
                    continue;
                }

                toTarget.Normalize();

                float targetAngle = (float)Math.Atan2(toTarget.Y, toTarget.X) + MathHelper.PiOver2;
                float angleDiff = MathHelper.WrapAngle(targetAngle - rotation);

                float rotationDelta = 0f;
                if (Math.Abs(angleDiff) > RotationDeadzone)
                {
                    float rotationInput = MathHelper.Clamp(angleDiff / MathHelper.PiOver4, -1f, 1f);
                    rotationDelta = rotationInput * moveStats.RotationSpeed * dt;
                }

                intent.Velocity = forward * moveStats.MaxSpeed;
                intent.RotationDelta = rotationDelta;
            }
        }
    }
}