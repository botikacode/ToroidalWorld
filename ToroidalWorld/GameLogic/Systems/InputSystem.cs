using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using ToroidalWorld.GameLogic.Entities.Components;
using ToroidalWorld.GameLogic.Physics.Components;
using ToroidalWorld.GameLogic.Session;

namespace ToroidalWorld.GameLogic.Systems
{
    public class InputSystem : EntityUpdateSystem
    {
        private readonly GameSession _session;

        private ComponentMapper<EntityFlagsComponent> _flagsMapper;
        private ComponentMapper<MoveStatsComponent> _moveStatsMapper;
        private ComponentMapper<BoatMoveComponent> _boatMoveMapper;

        private ComponentMapper<MovementIntent> _intentMapper;
        private ComponentMapper<MovementState> _stateMapper;
        private ComponentMapper<Transform2> _transformMapper;

        public InputSystem(GameSession session)
            : base(Aspect.All(typeof(EntityFlagsComponent), typeof(MoveStatsComponent), typeof(BoatMoveComponent), typeof(MovementIntent), typeof(MovementState), typeof(Transform2)))
        {
            _session = session;
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _flagsMapper = mapperService.GetMapper<EntityFlagsComponent>();
            _moveStatsMapper = mapperService.GetMapper<MoveStatsComponent>();
            _boatMoveMapper = mapperService.GetMapper<BoatMoveComponent>();

            _intentMapper = mapperService.GetMapper<MovementIntent>();
            _stateMapper = mapperService.GetMapper<MovementState>();
            _transformMapper = mapperService.GetMapper<Transform2>();
        }

        public override void Update(GameTime gameTime)
        {
            if (_session != null && !_session.IsWorldReady)
            {
                ClearPlayerIntents();
                return;
            }

            var keyboard = Keyboard.GetState();
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            for (int i = 0; i < ActiveEntities.Count; i++)
            {
                int entityId = ActiveEntities[i];

                var flags = _flagsMapper.Get(entityId);
                if (flags == null || !flags.Has(EntityFlags.Player))
                    continue;

                var moveStats = _moveStatsMapper.Get(entityId);
                var boatMove = _boatMoveMapper.Get(entityId);
                var intent = _intentMapper.Get(entityId);
                var state = _stateMapper.Get(entityId);
                var transform = _transformMapper.Get(entityId);

                if (moveStats == null || boatMove == null || intent == null || state == null || transform == null)
                    continue;

                Vector2 currentVel = state.Velocity;

                float rotation = transform.Rotation;
                Vector2 forward = new Vector2(
                    (float)System.Math.Cos(rotation - MathHelper.PiOver2),
                    (float)System.Math.Sin(rotation - MathHelper.PiOver2));

                Vector2 right = new Vector2(-forward.Y, forward.X);

                if (keyboard.IsKeyDown(Keys.W))
                    currentVel += forward * moveStats.Acceleration * dt;

                if (keyboard.IsKeyDown(Keys.S))
                    currentVel -= forward * moveStats.Acceleration * 0.5f * dt;

                currentVel *= boatMove.WaterDrag;

                float lateralSpeed = Vector2.Dot(currentVel, right);
                Vector2 lateralVelocity = right * lateralSpeed;
                currentVel -= lateralVelocity * boatMove.LateralFriction;

                float speed = currentVel.Length();
                if (speed > moveStats.MaxSpeed && speed > 0f)
                    currentVel = Vector2.Normalize(currentVel) * moveStats.MaxSpeed;

                float turnFactor = MathHelper.Clamp(speed / moveStats.MaxSpeed, 0.01f, 1f);

                float rotationInput = 0f;
                if (keyboard.IsKeyDown(Keys.A))
                    rotationInput = -1f;
                else if (keyboard.IsKeyDown(Keys.D))
                    rotationInput = 1f;

                float rotationDelta = rotationInput * moveStats.RotationSpeed * turnFactor * dt;

                intent.Velocity = currentVel;
                intent.RotationDelta = rotationDelta;
            }
        }

        private void ClearPlayerIntents()
        {
            for (int i = 0; i < ActiveEntities.Count; i++)
            {
                int entityId = ActiveEntities[i];

                var flags = _flagsMapper.Get(entityId);
                if (flags == null || !flags.Has(EntityFlags.Player))
                    continue;

                var intent = _intentMapper.Get(entityId);
                if (intent == null)
                    continue;

                intent.Velocity = Vector2.Zero;
                intent.RotationDelta = 0f;
            }
        }
    }
}