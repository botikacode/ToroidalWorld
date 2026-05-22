using Microsoft.Xna.Framework;
using MonoGame.Extended.ECS.Systems;
using System.Diagnostics;
using ToroidalWorld.GameLogic.Map;
using ToroidalWorld.Map;

namespace ToroidalWorld.GameLogic.Map.Systems
{
    public class ChunkGenerationSystem : UpdateSystem
    {
        private readonly MapData _mapData;
        private readonly PerlinNoise _noise;
        private readonly int _perlinSize;
        private readonly int _pixelSize = MapData.PixelSize;

        private const float perlinScale = 0.001f;
        private const int maxChunksPerUpdate = 2;

        public ChunkGenerationSystem(MapData mapData)
        {
            _mapData = mapData;
            _perlinSize = (int)(MapData.ChunkSize * _mapData.WorldSizeInChunks * perlinScale);
            _noise = new PerlinNoise(mapData.Seed, _perlinSize);

            Debug.WriteLine($"Perlin noise initialized with Size: {MapData.ChunkSize * _mapData.WorldSizeInChunks}Perlizsize {_perlinSize} and seed {mapData.Seed}");
        }

        public override void Update(GameTime gametime)
        {
            for (int i = 0; i < maxChunksPerUpdate; i++)
            {
                if (_mapData.GenerationQueue.Count == 0)
                    break;

                Point id = _mapData.GenerationQueue.Dequeue();

                var existing = _mapData.GetChunk(id);
                if (existing != null && !existing.NeedsRebuild)
                    continue;

                var chunk = existing ?? _mapData.CreateChunk(id);

                if (_mapData.TryGetModifiedChunk(id, out var flatTiles))
                {
                    MapData.CopyFlatTo2D(flatTiles, chunk.Tiles);
                    chunk.State = ChunkState.DataReady;
                    chunk.NeedsRebuild = true;
                    continue;
                }

                //If there is no modified chunk, regenerate using Perlin noise (only if new or NeedsRebuild)
                GenerateChunkData(chunk);

                chunk.State = ChunkState.DataReady;
                chunk.NeedsRebuild = true;
            }
        }

        private void GenerateChunkData(Chunk chunk)
        {
            int size = MapData.ChunkSize;

            Point chunkXSize = new Point(chunk.Id.X * size, chunk.Id.Y * size);

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    int mapX = (chunkXSize.X + x) * _pixelSize;
                    int mapY = (chunkXSize.Y + y) * _pixelSize;
                    chunk.Tiles[x, y] = (int)(((_noise.FractalNoise(mapX * perlinScale, mapY * perlinScale, 7, 0.5f) + 1) / 2) * 100);
                }
            }
        }
    }
}