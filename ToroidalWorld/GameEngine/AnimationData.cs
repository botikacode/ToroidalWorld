using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToroidalWorld.GameEngine
{
    public class AnimationData
    {
        public string Name { get; set; }
        public int[] Frames { get; set; }
        public float FrameDuration { get; set; }
        public bool IsLooping { get; set; }
    }
}
