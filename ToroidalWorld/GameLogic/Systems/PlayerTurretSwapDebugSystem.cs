using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using MonoGame.Extended.Input;
using System;
using ToroidalWorld.GameLogic.Entities.Components;
using ToroidalWorld.GameLogic.Session;

namespace ToroidalWorld.GameLogic.Entities.Systems
{
    public sealed class PlayerTurretSwapDebugSystem : UpdateSystem
    {
        private readonly GameSession _session;

        public PlayerTurretSwapDebugSystem(GameSession session)
        {
            _session = session;
        }

        public override void Update(GameTime gameTime)
        {
            if (_session == null || _session.World == null || !_session.IsWorldReady || !_session.HasPlayer)
                return;

            var keyboard = KeyboardExtended.GetState();
            if (!keyboard.WasKeyPressed(Keys.Space))
                return;

            Entity player;

            try
            {
                player = _session.World.GetEntity(_session.PlayerEntityId);
            }
            catch
            {
                return;
            }

            if (player == null)
                return;

            var mounts = player.Get<TurretMountsComponent>();
            if (mounts == null || mounts.Mounts.Count == 0 || mounts.Mounts[0] == null)
                return;

            string current = mounts.Mounts[0].TurretName;

            string next = string.Equals(current, "Turret", StringComparison.OrdinalIgnoreCase)
                ? "MissileTurret"
                : "Turret";

            _session.TrySwapPlayerTurret(mountIndex: 0, turretName: next);
        }
    }
}