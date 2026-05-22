using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToroidalWorld.GameLogic.Entities.Components
{
    public class LifeTimeComponent
    {
        public float RemainingTime { get; set; }
        public LifeTimeComponent(float initialTime)
        {
            RemainingTime = initialTime;
        }
    }
}
