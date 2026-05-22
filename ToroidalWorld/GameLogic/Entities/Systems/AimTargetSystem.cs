using System;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using ToroidalWorld.GameLogic.Components;
using ToroidalWorld.GameLogic.Entities.Components;
using ToroidalWorld.GameLogic.Map;
using ToroidalWorld.GameLogic.Session;

namespace ToroidalWorld.GameLogic.Entities.Systems
{
    public sealed class AimAtTargetSystem : EntityUpdateSystem
    {
        private readonly GameSession _session;
        private readonly MapData _map;

        private ComponentMapper<ChildOfComponent> _childOfMapper;
        private ComponentMapper<TurretAimComponent> _turretAimMapper;
        private ComponentMapper<TargetingComponent> _targetingMapper;

        public AimAtTargetSystem(GameSession session, MapData map)
            : base(Aspect.All(typeof(ChildOfComponent), typeof(TurretAimComponent), typeof(TargetingComponent)))
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _map = map ?? throw new ArgumentNullException(nameof(map));
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _childOfMapper = mapperService.GetMapper<ChildOfComponent>();
            _turretAimMapper = mapperService.GetMapper<TurretAimComponent>();
            _targetingMapper = mapperService.GetMapper<TargetingComponent>();
        }

        public override void Update(GameTime gameTime)
        {
            var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            foreach (var entityId in ActiveEntities)
            {
                var childOf = _childOfMapper.Get(entityId);
                var turretAim = _turretAimMapper.Get(entityId);
                var targeting = _targetingMapper.Get(entityId);

                if (childOf == null || turretAim == null || targeting == null)
                    continue;

                var desiredLocal = turretAim.RestLocalRotation;

                if (targeting.TargetEntityId >= 0)
                {
                    Transform2 parentTransform;
                    Transform2 targetTransform;

                    try
                    {
                        parentTransform = _session.World.GetEntity(childOf.ParentEntityId)?.Get<Transform2>();
                        targetTransform = _session.World.GetEntity(targeting.TargetEntityId)?.Get<Transform2>();
                    }
                    catch
                    {
                        parentTransform = null;
                        targetTransform = null;
                    }

                    if (parentTransform != null && targetTransform != null)
                    {
                        var rotatedOffset = Vector2.Transform(childOf.LocalOffset, Matrix.CreateRotationZ(parentTransform.Rotation));
                        var turretWorldPos = parentTransform.Position + rotatedOffset;

                        var targetWorldPos = _map.GetToroidalPosition(targetTransform.Position, turretWorldPos);

                        var desiredGlobal =
                            (float)Math.Atan2(targetWorldPos.Y - turretWorldPos.Y, targetWorldPos.X - turretWorldPos.X)
                            + MathHelper.PiOver2
                            + turretAim.AimOffsetRadians;

                        desiredLocal = MathHelper.WrapAngle(desiredGlobal - parentTransform.Rotation);
                    }
                }

                var currentLocal = childOf.LocalRotation;
                var delta = MathHelper.WrapAngle(desiredLocal - currentLocal);

                var maxStep = turretAim.TurnSpeed * dt;
                if (delta > maxStep) delta = maxStep;
                else if (delta < -maxStep) delta = -maxStep;

                childOf.LocalRotation = MathHelper.WrapAngle(currentLocal + delta);
            }
        }
    }
}