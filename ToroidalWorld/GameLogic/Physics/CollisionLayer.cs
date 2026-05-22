using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToroidalWorld.GameLogic.Physics
{
    [Flags]
    public enum CollisionLayer
    {
        None = 0,
        Player = 1 << 0,  // 1
        Enemy = 1 << 1,  // 2
        PlayerProjectile = 1 << 2,  // 4
        EnemyProjectile = 1 << 3,  // 8
    }
}
