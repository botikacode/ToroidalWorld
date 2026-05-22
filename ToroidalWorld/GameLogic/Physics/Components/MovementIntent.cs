using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToroidalWorld.GameLogic.Physics.Components
{
    public class MovementIntent
    {
        public Vector2 Velocity;
        public float RotationDelta;

        public MovementIntent()
        {
            Velocity = Vector2.Zero;
            RotationDelta = 0;
        }

        public MovementIntent(Vector2 velocity, float rotationDelta)
        {
            Velocity = velocity;
            RotationDelta = rotationDelta;
        }
    }
}
