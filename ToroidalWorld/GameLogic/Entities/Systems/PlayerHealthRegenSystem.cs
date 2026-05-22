using Microsoft.Xna.Framework;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using System;
using ToroidalWorld.GameLogic.Entities.Components;

namespace ToroidalWorld.GameLogic.Entities.Systems
{
    public sealed class PlayerHealthRegenSystem : EntityUpdateSystem
    {
        private ComponentMapper<EntityFlagsComponent> _flagsMapper;
        private ComponentMapper<HealthComponent> _healthMapper;
        private ComponentMapper<HealthRegenComponent> _regenMapper;

        public PlayerHealthRegenSystem()
            : base(Aspect.All(typeof(EntityFlagsComponent), typeof(HealthComponent), typeof(HealthRegenComponent)))
        {
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _flagsMapper = mapperService.GetMapper<EntityFlagsComponent>();
            _healthMapper = mapperService.GetMapper<HealthComponent>();
            _regenMapper = mapperService.GetMapper<HealthRegenComponent>();
        }

        public override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (dt <= 0f)
                return;

            for (int i = 0; i < ActiveEntities.Count; i++)
            {
                int entityId = ActiveEntities[i];

                var flags = _flagsMapper.Get(entityId);
                if (flags == null || !flags.Has(EntityFlags.Player))
                    continue;

                var health = _healthMapper.Get(entityId);
                var regen = _regenMapper.Get(entityId);

                if (health == null || regen == null)
                    continue;

                if (health.IsDead)
                    continue;

                if (health.CurrentHealth >= health.MaxHealth)
                {
                    regen.AccumulatedHealing = 0f;
                    continue;
                }

                float regenPerSecond = regen.RegenPerSecond;
                if (regenPerSecond <= 0f)
                    continue;

                regen.AccumulatedHealing += regenPerSecond * dt;

                int heal = (int)MathF.Floor(regen.AccumulatedHealing);
                if (heal <= 0)
                    continue;

                regen.AccumulatedHealing -= heal;

                int newHealth = health.CurrentHealth + heal;
                if (newHealth > health.MaxHealth)
                    newHealth = health.MaxHealth;

                health.CurrentHealth = newHealth;

                if (health.CurrentHealth >= health.MaxHealth)
                    regen.AccumulatedHealing = 0f;
            }
        }
    }
}