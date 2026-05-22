using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using ToroidalWorld.GameLogic.Entities.Components;

namespace ToroidalWorld.GameLogic.Entities.Systems
{
    public sealed class PlayerStatsApplySystem : EntityUpdateSystem
    {
        private const float MinCooldownSeconds = 0.01f;
        private const float MinRateMultiplier = 0.01f;

        private ComponentMapper<EntityFlagsComponent> _flagsMapper;
        private ComponentMapper<MoveStatsComponent> _moveStatsMapper;
        private ComponentMapper<BaseStatsComponent> _baseStatsMapper;
        private ComponentMapper<StatMultipliersComponent> _multipliersMapper;

        private ComponentMapper<TurretMountsComponent> _turretMountsMapper;
        private ComponentMapper<TurretAimComponent> _turretAimMapper;
        private ComponentMapper<WeaponComponent> _weaponMapper;
        private ComponentMapper<TargetingComponent> _targetingMapper;

        public PlayerStatsApplySystem()
            : base(Aspect.All(
                typeof(EntityFlagsComponent),
                typeof(MoveStatsComponent),
                typeof(BaseStatsComponent),
                typeof(StatMultipliersComponent)))
        {
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _flagsMapper = mapperService.GetMapper<EntityFlagsComponent>();
            _moveStatsMapper = mapperService.GetMapper<MoveStatsComponent>();
            _baseStatsMapper = mapperService.GetMapper<BaseStatsComponent>();
            _multipliersMapper = mapperService.GetMapper<StatMultipliersComponent>();

            _turretMountsMapper = mapperService.GetMapper<TurretMountsComponent>();
            _turretAimMapper = mapperService.GetMapper<TurretAimComponent>();
            _weaponMapper = mapperService.GetMapper<WeaponComponent>();
            _targetingMapper = mapperService.GetMapper<TargetingComponent>();
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            for (int i = 0; i < ActiveEntities.Count; i++)
            {
                int entityId = ActiveEntities[i];

                var flags = _flagsMapper.Get(entityId);
                if (flags == null || !flags.Has(EntityFlags.Player))
                    continue;

                var move = _moveStatsMapper.Get(entityId);
                var baseStats = _baseStatsMapper.Get(entityId);
                var mult = _multipliersMapper.Get(entityId);

                if (move == null || baseStats == null || mult == null)
                    continue;

                ApplyPlayerMove(move, baseStats, mult);
                ApplyPlayerTurrets(entityId, mult);
            }
        }

        private static void ApplyPlayerMove(MoveStatsComponent move, BaseStatsComponent baseStats, StatMultipliersComponent mult)
        {
            float moveMult = mult.MoveSpeedMultiplier;
            if (moveMult < 0f)
                moveMult = 0f;

            move.MaxSpeed = baseStats.MaxSpeed * moveMult;
            move.Acceleration = baseStats.Acceleration * moveMult;
            move.RotationSpeed = baseStats.RotationSpeed;
        }

        private void ApplyPlayerTurrets(int playerEntityId, StatMultipliersComponent mult)
        {
            var mounts = _turretMountsMapper.Get(playerEntityId);
            if (mounts == null || mounts.Mounts.Count == 0)
                return;

            float aimMult = mult.TurretAimSpeedMultiplier;
            if (aimMult < 0f)
                aimMult = 0f;

            float rangeMult = mult.TurretRangeMultiplier;
            if (rangeMult < 0f)
                rangeMult = 0f;

            float cooldownRateMult = mult.TurretCooldownRateMultiplier;
            if (cooldownRateMult < MinRateMultiplier)
                cooldownRateMult = MinRateMultiplier;

            for (int i = 0; i < mounts.Mounts.Count; i++)
            {
                var mount = mounts.Mounts[i];
                if (mount == null)
                    continue;

                int turretId = mount.TurretEntityId;
                if (turretId < 0)
                    continue;

                var turretBase = _baseStatsMapper.Get(turretId);
                if (turretBase == null)
                    continue;

                var aim = _turretAimMapper.Get(turretId);
                if (aim != null && turretBase.TurretTurnSpeed > 0f)
                    aim.TurnSpeed = turretBase.TurretTurnSpeed * aimMult;

                var targeting = _targetingMapper.Get(turretId);
                if (targeting != null && turretBase.TurretRangeTiles > 0f)
                    targeting.MaxRange = turretBase.TurretRangeTiles * rangeMult;

                var weapon = _weaponMapper.Get(turretId);
                if (weapon != null && turretBase.TurretCooldownSeconds > 0f)
                {
                    weapon.CooldownSeconds = turretBase.TurretCooldownSeconds / cooldownRateMult;

                    if (weapon.CooldownSeconds < MinCooldownSeconds)
                        weapon.CooldownSeconds = MinCooldownSeconds;

                    if (weapon.RemainingSeconds > weapon.CooldownSeconds)
                        weapon.RemainingSeconds = weapon.CooldownSeconds;
                }
            }
        }
    }
}