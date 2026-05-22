using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToroidalWorld.GameEngine
{
    public struct SpriteData
    {
        public List<AnimationData> Animations { get; set; }
        public int FrameWidth { get; set; }
        public int FrameHeight { get; set; }

    }
}