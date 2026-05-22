using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToroidalWorld.GameLogic.Entities.Components
{
    public class MoveStatsComponent
    {
        public float MaxSpeed { get; set; }
        public float Acceleration { get; set; }
        public float RotationSpeed { get; set; }

        public MoveStatsComponent()
        {
            MaxSpeed = 1f;
            Acceleration = 1f;
            RotationSpeed = 1f;
        }

        public MoveStatsComponent(float maxSpeed, float acceleration = 1f, float rotationSpeed = 1f)
        {
            MaxSpeed = maxSpeed;
            Acceleration = acceleration;
            RotationSpeed = rotationSpeed;
        }
    }
}
