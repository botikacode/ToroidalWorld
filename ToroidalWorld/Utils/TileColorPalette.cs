using Microsoft.Xna.Framework;

namespace ToroidalWorld.Utils
{

    public static class TileColorPalette
    {
        private const int MaxValue = 100;
        private static readonly Color[] _palette = CreatePalette();
        private static readonly uint[] _packedPalette = CreatePackedPalette();

        private static Color[] CreatePalette()
        {
            var palette = new Color[MaxValue + 1];
            for (int v = 0; v <= MaxValue; v++)
            {
                palette[v] = CalculateColor(v);
            }
            return palette;
        }

        private static uint[] CreatePackedPalette()
        {
            var packed = new uint[MaxValue + 1];
            for (int v = 0; v <= MaxValue; v++)
            {
                packed[v] = _palette[v].PackedValue;
            }
            return packed;
        }

        /*private static Color CalculateColor(int value)
        {
            if (value < 10) return new Color(0, 0, 51);
            if (value < 20) return new Color(0, 0, 102);
            if (value < 35) return new Color(0, 0, 153);
            if (value < 48) return new Color(0, 0, 204);
            if (value < 54) return new Color(0, 0, 255);
            if (value < 55) return Color.SandyBrown;
            if (value < 60) return Color.GreenYellow;
            if (value < 70) return Color.ForestGreen;
            if (value < 80) return Color.Brown;
            if (value < 90) return Color.Gray;
            return Color.White;
        }*/

        private static Color CalculateColor(int value)
        {
            if (value < 10) return new Color(0, 0, 51);
            if (value < 20) return new Color(0, 0, 102);
            if (value < 35) return new Color(0, 0, 153);
            if (value < 48) return new Color(0, 0, 204);
            if (value < 54) return new Color(0, 0, 255);
            if (value < 55) return new Color(122,224,255);
            if (value < 57) return new Color(188, 239, 255);
            if (value < 60) return new Color(210, 245, 255);
            if (value < 65) return new Color(230, 250, 255);
            return Color.White;
        }


        public static Color GetColor(int value)
        {
            int v = System.Math.Clamp(value, 0, MaxValue);
            return _palette[v];
        }

        public static uint GetPackedColor(int value)
        {
            int v = System.Math.Clamp(value, 0, MaxValue);
            return _packedPalette[v];
        }
    }
}