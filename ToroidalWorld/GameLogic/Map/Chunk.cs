using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ToroidalWorld.Map
{
    public enum ChunkState
    {
        Empty,
        DataReady,
        TextureReady
    }

    public class Chunk
    {
        public Point Id;

        public int[,] Tiles;

        public RenderTarget2D RenderTarget;

        public ChunkState State = ChunkState.Empty;

        public bool NeedsRebuild = false;

        public Chunk(Point id, int chunkSize)
        {
            Id = id;
            Tiles = new int[chunkSize, chunkSize];
        }
    }
}