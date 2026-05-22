using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace ToroidalWorld.GameLogic.Entities.Components
{
    public sealed class TurretMountsComponent
    {
        public List<TurretMountState> Mounts { get; } = new();
    }

    public sealed class TurretMountState
    {
        public TurretMountState(Vector2 localOffset, float localRotationRadians, int turretEntityId, string turretName)
        {
            LocalOffset = localOffset;
            LocalRotationRadians = localRotationRadians;
            TurretEntityId = turretEntityId;
            TurretName = turretName;
        }

        public Vector2 LocalOffset { get; }

        public float LocalRotationRadians { get; }

        public int TurretEntityId { get; set; }

        public string TurretName { get; set; }
    }
}