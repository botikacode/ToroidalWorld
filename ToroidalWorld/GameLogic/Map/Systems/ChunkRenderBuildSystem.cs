using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.ECS.Systems;
using System.Buffers;
using ToroidalWorld.Map;
using ToroidalWorld.Utils;

namespace ToroidalWorld.GameLogic.Map.Systems
{
    public class ChunkRenderBuildSystem : DrawSystem
    {
        private MapData _mapData;
        private GraphicsDevice _graphicsDevice;

        private const int maxBuildPerFrame = 6;

        public ChunkRenderBuildSystem(
            MapData mapData,
            GraphicsDevice graphicsDevice)
        {
            this._mapData = mapData;
            this._graphicsDevice = graphicsDevice;
        }

        public override void Draw(GameTime gametime)
        {
            int built = 0;

            foreach (var chunk in _mapData.GetAllChunks())
            {
                if (built >= maxBuildPerFrame)
                    break;

                if (chunk.State != ChunkState.DataReady &&
                    !chunk.NeedsRebuild)
                    continue;

                BuildChunk(chunk);
                built++;
            }
        }

        private void BuildChunk(Chunk chunk)
        {
            int size = MapData.ChunkSize;

            if (chunk.RenderTarget == null)
            {
                chunk.RenderTarget = new RenderTarget2D(_graphicsDevice, size, size, false, SurfaceFormat.Color, DepthFormat.None);
            }

            int total = size * size;
            var pool = ArrayPool<Color>.Shared;
            Color[] tileColors = pool.Rent(total);

            try
            {
                // Refill the buffer (row by row) without new allocations
                for (int y = 0; y < size; y++)
                {
                    int rowOffset = y * size;
                    for (int x = 0; x < size; x++)
                    {
                        int tileType = chunk.Tiles[x, y];
                        tileColors[rowOffset + x] = TileColorPalette.GetColor(tileType);
                    }
                }

                // Ensure no RT is active before SetData
                _graphicsDevice.SetRenderTarget(null);
                chunk.RenderTarget.SetData(tileColors, 0, total);
            }
            finally
            {
                // Return to the pool; DO NOT clear by default for performance.
                pool.Return(tileColors, clearArray: false);
            }

            chunk.State = ChunkState.TextureReady;
            chunk.NeedsRebuild = false;
        }
    }
}
