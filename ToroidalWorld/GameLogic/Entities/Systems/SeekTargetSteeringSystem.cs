using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using System;
using System.Collections.Generic;
using ToroidalWorld.GameLogic.Entities.Components;
using ToroidalWorld.GameLogic.Physics.Components;
using ToroidalWorld.GameLogic.Session;

namespace ToroidalWorld.GameLogic.Physics.Systems
{
    public sealed class SeekTargetSteeringSystem : EntityUpdateSystem
    {
        private ComponentMapper<EntityFlagsComponent> _flagsMapper;
        private ComponentMapper<MoveStatsComponent> _moveStatsMapper;
        private ComponentMapper<MovementIntent> _intentMapper;
        private ComponentMapper<MovementState> _stateMapper;
        private ComponentMapper<Transform2> _transformMapper;
        private ComponentMapper<TargetingComponent> _targetingMapper;

        private readonly GameSession _gameSession;

        private const float StopDistance = 5f;
        private const float RotationDeadzone = 0.12f;
        private const float AlignmentThreshold = 0.82f;
        private const float MinSpeedMultiplier = 0.7f;
        private const float StopCooldown = 0.01f;

        private const float MinMoveFactor = 0.01f;
        private const float MinRealSpeed = 2f;

        private class SeekState
        {
            public bool IsStopped;
            public float StopTimer;
            public float BlockedTimer;
        }

        private readonly Dictionary<int, SeekState> _states = new Dictionary<int, SeekState>();

        public SeekTargetSteeringSystem(GameSession gameSession)
            : base(Aspect.All(typeof(EntityFlagsComponent), typeof(MoveStatsComponent), typeof(MovementIntent), typeof(MovementState), typeof(Transform2), typeof(TargetingComponent)))
        {
            _gameSession = gameSession ?? throw new ArgumentNullException(nameof(gameSession));
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _flagsMapper = mapperService.GetMapper<EntityFlagsComponent>();
            _moveStatsMapper = mapperService.GetMapper<MoveStatsComponent>();
            _intentMapper = mapperService.GetMapper<MovementIntent>();
            _stateMapper = mapperService.GetMapper<MovementState>();
            _transformMapper = mapperService.GetMapper<Transform2>();
            _targetingMapper = mapperService.GetMapper<TargetingComponent>();
        }

        public override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            foreach (var entityId in ActiveEntities)
            {
                var flags = _flagsMapper.Get(entityId);
                if (flags == null)
                    continue;

                if (flags.Has(EntityFlags.Projectile))
                    continue;

                var moveStats = _moveStatsMapper.Get(entityId);
                var intent = _intentMapper.Get(entityId);
                var state = _stateMapper.Get(entityId);
                var transform = _transformMapper.Get(entityId);
                var targeting = _targetingMapper.Get(entityId);

                if (moveStats == null || intent == null || state == null || transform == null || targeting == null)
                    continue;

                if (targeting.TargetEntityId < 0)
                {
                    intent.Velocity = Vector2.Zero;
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
                        intent.Velocity = Vector2.Zero;
                        intent.RotationDelta = 0f;
                        continue;
                    }

                    targetPosition = targetTransform.Position;
                }
                catch
                {
                    intent.Velocity = Vector2.Zero;
                    intent.RotationDelta = 0f;
                    continue;
                }

                if (!_states.TryGetValue(entityId, out var seekState))
                {
                    seekState = new SeekState();
                    _states[entityId] = seekState;
                }

                if (seekState.StopTimer > 0f)
                    seekState.StopTimer -= dt;

                float rotation = transform.Rotation;
                Vector2 forward = new Vector2(
                    (float)Math.Cos(rotation - MathHelper.PiOver2),
                    (float)Math.Sin(rotation - MathHelper.PiOver2));

                Vector2 toTarget = targetPosition - transform.Position;
                float distanceToTarget = toTarget.Length();

                Vector2 frameDisplacement = state.ProposedPosition - state.OldPosition;
                float realSpeed = frameDisplacement.Length() / Math.Max(dt, 0.0001f);
                float desiredSpeed = state.Velocity.Length();

                bool tryingToMove = desiredSpeed > 0.01f;
                bool almostStill = realSpeed < MinRealSpeed;
                bool movingMuchLessThanDesired = realSpeed < desiredSpeed * MinMoveFactor;

                if (tryingToMove && almostStill && movingMuchLessThanDesired)
                    seekState.BlockedTimer += dt;
                else
                    seekState.BlockedTimer = 0f;

                bool isBlocked = seekState.BlockedTimer > 0.2f;

                if (isBlocked)
                {
                    seekState.IsStopped = true;
                    seekState.StopTimer = StopCooldown;

                    intent.Velocity = Vector2.Zero;
                    intent.RotationDelta = 0f;
                    state.Velocity = Vector2.Zero;
                    continue;
                }

                if (seekState.StopTimer > 0f)
                {
                    Vector2 currentVelocity = state.Velocity;
                    intent.Velocity = currentVelocity * 0.7f;
                    intent.RotationDelta = 0f;
                    continue;
                }

                if (distanceToTarget > StopDistance)
                {
                    seekState.IsStopped = false;

                    toTarget.Normalize();

                    float targetAngle = (float)Math.Atan2(toTarget.Y, toTarget.X) + MathHelper.PiOver2;
                    float angleDiff = MathHelper.WrapAngle(targetAngle - rotation);

                    float rotationDelta = 0f;
                    if (Math.Abs(angleDiff) > RotationDeadzone)
                    {
                        float rotationInput = MathHelper.Clamp(angleDiff / MathHelper.PiOver4, -1f, 1f);
                        rotationDelta = rotationInput * moveStats.RotationSpeed * dt;
                    }

                    float forwardDot = Vector2.Dot(toTarget, forward);
                    Vector2 desiredVelocity;

                    if (forwardDot > AlignmentThreshold)
                    {
                        float speedMultiplier = MathHelper.Clamp(distanceToTarget / 120f, MinSpeedMultiplier, 1f);
                        desiredVelocity = forward * moveStats.MaxSpeed * speedMultiplier;
                    }
                    else
                    {
                        desiredVelocity = Vector2.Zero;
                    }

                    intent.Velocity = desiredVelocity;
                    intent.RotationDelta = rotationDelta;
                }
                else
                {
                    seekState.IsStopped = true;
                    seekState.StopTimer = StopCooldown;

                    intent.Velocity = Vector2.Zero;
                    intent.RotationDelta = 0f;
                    state.Velocity = Vector2.Zero;
                }
            }
        }
    }
}