using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToroidalWorld.GameLogic.Physics.Components
{
    public class TerrainCollider
    {
        public int Height;
        public int Width;

        public bool Rotates;

        public TerrainCollider(int size, bool rotates = false)
        {
            Height = size;
            Width = size;
            Rotates = rotates;
        }

        public TerrainCollider(int height, int width, bool rotates = false)
        {
            Height = height;
            Width = width;
            Rotates = rotates;
        }
    }
}
