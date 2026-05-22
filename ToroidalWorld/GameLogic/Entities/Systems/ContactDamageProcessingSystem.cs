using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using ToroidalWorld.GameEngine;
using ToroidalWorld.GameLogic.Entities.Components;
using ToroidalWorld.GameLogic.Physics;
using ToroidalWorld.GameLogic.Session;

namespace ToroidalWorld.GameLogic.Entities.Systems
{
    public sealed class ContactDamageProcessingSystem : UpdateSystem
    {
        private readonly GameSession _session;
        private readonly DerivedCollisionEvents _derived;

        private const string PlayerHitSoundKey = "PlayerHit";

        public ContactDamageProcessingSystem(GameSession session, DerivedCollisionEvents derived)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _derived = derived ?? throw new ArgumentNullException(nameof(derived));
        }

        public override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            TickCooldowns(dt);

            for (int i = 0; i < _derived.ContactDamages.Count; i++)
            {
                var evt = _derived.ContactDamages[i];

                if (!TryGetEntity(evt.AttackerId, out var attacker))
                    continue;

                if (!TryGetEntity(evt.VictimId, out var victim))
                    continue;

                var attackerDamage = attacker.Get<DamageComponent>();
                var victimHealth = victim.Get<HealthComponent>();
                var victimCooldown = victim.Get<DamageTakenCooldownComponent>();

                if (attackerDamage == null || victimHealth == null || victimCooldown == null)
                    continue;

                if (victimCooldown.RemainingSeconds > 0f)
                    continue;

                victimHealth.CurrentHealth -= attackerDamage.Damage;
                victimCooldown.RemainingSeconds = victimCooldown.CooldownSeconds;

                if (evt.VictimId == _session.PlayerEntityId)
                    AudioManager.TryPlaySoundEffect(PlayerHitSoundKey);
            }

            _derived.ContactDamages.Clear();
        }

        private void TickCooldowns(float dt)
        {
            if (_session.PlayerEntityId < 0)
                return;

            try
            {
                var player = _session.World.GetEntity(_session.PlayerEntityId);
                var cd = player?.Get<DamageTakenCooldownComponent>();
                if (cd == null)
                    return;

                cd.RemainingSeconds -= dt;
                if (cd.RemainingSeconds < 0f)
                    cd.RemainingSeconds = 0f;
            }
            catch
            {
                // ignorar
            }
        }

        private bool TryGetEntity(int entityId, out Entity entity)
        {
            entity = null;

            try { entity = _session.World.GetEntity(entityId); }
            catch { return false; }

            return entity != null;
        }
    }
}