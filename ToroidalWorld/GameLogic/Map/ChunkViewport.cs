using System;
using MonoGame.Extended;

namespace ToroidalWorld.GameLogic.Map
{
    public static class ChunkViewport
    {
        public static void GetChunkBoundsFromCamera(
            OrthographicCamera camera,
            int chunkPixels,
            int extraRadiusChunks,
            out int firstChunkX,
            out int lastChunkX,
            out int firstChunkY,
            out int lastChunkY)
        {
            if (camera == null) throw new ArgumentNullException(nameof(camera));
            if (chunkPixels <= 0) throw new ArgumentOutOfRangeException(nameof(chunkPixels));

            var bounds = camera.BoundingRectangle;

            firstChunkX = (int)Math.Floor(bounds.Left / (float)chunkPixels) - extraRadiusChunks;
            lastChunkX = (int)Math.Floor(bounds.Right / (float)chunkPixels) + extraRadiusChunks;

            firstChunkY = (int)Math.Floor(bounds.Top / (float)chunkPixels) - extraRadiusChunks;
            lastChunkY = (int)Math.Floor(bounds.Bottom / (float)chunkPixels) + extraRadiusChunks;
        }
    }
}