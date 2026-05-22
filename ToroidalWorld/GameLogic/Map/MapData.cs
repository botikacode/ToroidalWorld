using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using ToroidalWorld.GameLogic.Entities;
using ToroidalWorld.Map;

namespace ToroidalWorld.GameLogic.Map
{
    public class MapData
    {
        public const int EntitySpawnDistance = 50;
        public const int PixelSize = 2;
        public const int ChunkSize = 64;
        public readonly int WorldSizeInChunks = 125; // min 125, max 250000
        public readonly int WorldSizeInPixels;

        public Queue<Point> GenerationQueue { get; } = new();

        public readonly int Seed;
        public readonly string Name;

        private Dictionary<Point, Chunk> _chunks = new Dictionary<Point, Chunk>();
        private Dictionary<Point, List<int>> _activeEntitiesForChunk = new Dictionary<Point, List<int>>();
        private Dictionary<Point, List<WorldEntitySnapshot>> _inactiveEntitiesForChunk = new Dictionary<Point, List<WorldEntitySnapshot>>();

        private readonly Dictionary<Point, int[]> _modifiedChunks = new Dictionary<Point, int[]>();

        public MapData(int seed, string name)
        {
            Seed = seed;
            Name = name;

            WorldSizeInPixels = WorldSizeInChunks * ChunkSize * PixelSize;
        }

        //Terraforming above a tile (in global tile coordinates, not world pixel coordinates)
        public void ApplyDeltaToTile(int tileX, int tileY, int delta)
        {
            var wrappedWorldPx = WrapWorldCoordinates(tileX, tileY);
            int wrappedTileX = wrappedWorldPx.X / PixelSize;
            int wrappedTileY = wrappedWorldPx.Y / PixelSize;

            ApplyDeltaToTile_NoAlloc(wrappedTileX, wrappedTileY, delta);
        }

        // Explosion centered in a tile, affecting all tiles within radiusInTiles (inclusive) with the same delta. Uses toroidal wrapping.
        public void ApplyExplosion(Point mapTilePosition, int radiusInTiles)
        {
            if (radiusInTiles <= 0)
                return;

            var centerWorldPx = WrapWorldCoordinates(mapTilePosition.X, mapTilePosition.Y);

            int centerTileX = centerWorldPx.X / PixelSize;
            int centerTileY = centerWorldPx.Y / PixelSize;

            int r = radiusInTiles;
            int rSq = r * r;

            // Iterate over the bounding box of the circle in global tile coordinates (toroidal)
            for (int dy = -r; dy <= r; dy++)
            {
                for (int dx = -r; dx <= r; dx++)
                {
                    int distSq = (dx * dx) + (dy * dy);
                    if (distSq > rSq)
                        continue;

                    int tileX = centerTileX + dx;
                    int tileY = centerTileY + dy;

                    var wrappedWorldPx = WrapWorldCoordinates(tileX, tileY);
                    int wrappedTileX = wrappedWorldPx.X / PixelSize;
                    int wrappedTileY = wrappedWorldPx.Y / PixelSize;

                    ApplyDeltaToTile_NoAlloc(wrappedTileX, wrappedTileY, -50);
                }
            }
        }

        private void ApplyDeltaToTile_NoAlloc(int tileX, int tileY, int delta)
        {
            int chunkX = Math.DivRem(tileX, ChunkSize, out int localX);
            int chunkY = Math.DivRem(tileY, ChunkSize, out int localY);

            var chunkId = WrapChunkCoordinates(chunkX, chunkY);

            Chunk chunk = GetChunk(chunkId);
            if (chunk == null)
            {
                return;
            }

            int current = chunk.Tiles[localX, localY];
            int next = current + delta;

            if (current < 53) next = current;
            else if (next < 53) next = 53;

            if (next == current)
                return;

            chunk.Tiles[localX, localY] = next;

            chunk.NeedsRebuild = true;

            UpsertModifiedChunkTile(chunkId, localX, localY, next);
        }

        private void UpsertModifiedChunkTile(Point chunkId, int localX, int localY, int newValue)
        {
            if (!_modifiedChunks.TryGetValue(chunkId, out var flat))
            {
                flat = new int[ChunkSize * ChunkSize];

                var loaded = GetChunk(chunkId);
                if (loaded != null)
                {
                    int i = 0;
                    for (int y = 0; y < ChunkSize; y++)
                    {
                        for (int x = 0; x < ChunkSize; x++)
                        {
                            flat[i++] = loaded.Tiles[x, y];
                        }
                    }
                }

                _modifiedChunks[chunkId] = flat;
            }

            flat[(localY * ChunkSize) + localX] = newValue;
        }

        // Modified chunk management

        public bool TryGetModifiedChunk(Point chunkId, out int[] tilesFlat)
        {
            return _modifiedChunks.TryGetValue(chunkId, out tilesFlat);
        }

        public void SetModifiedChunk(Point chunkId, int[] tilesFlat)
        {
            if (tilesFlat == null) throw new ArgumentNullException(nameof(tilesFlat));
            if (tilesFlat.Length != ChunkSize * ChunkSize)
                throw new ArgumentException($"Expected {ChunkSize * ChunkSize} tiles.", nameof(tilesFlat));

            _modifiedChunks[chunkId] = tilesFlat;

            if (_chunks.TryGetValue(chunkId, out var loadedChunk))
                loadedChunk.NeedsRebuild = true;
        }

        public static void CopyFlatTo2D(int[] flat, int[,] dest)
        {
            if (flat == null) throw new ArgumentNullException(nameof(flat));
            if (dest == null) throw new ArgumentNullException(nameof(dest));

            int width = dest.GetLength(0);
            int height = dest.GetLength(1);

            if (flat.Length != width * height)
                throw new ArgumentException("Flat array length does not match destination dimensions.", nameof(flat));

            int i = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    dest[x, y] = flat[i++];
                }
            }
        }

        // Chunk Map Management

        public bool HasChunk(Point id)
        {
            return _chunks.ContainsKey(id);
        }

        public Chunk GetChunk(Point id)
        {
            _chunks.TryGetValue(id, out var chunk);
            return chunk;
        }

        public Chunk CreateChunk(Point id)
        {
            var chunk = new Chunk(id, ChunkSize);
            _chunks[id] = chunk;
            return chunk;
        }

        public IEnumerable<Chunk> GetAllChunks()
        {
            return _chunks.Values;
        }

        public void UnloadFarChunks(Point center, int unloadRadius)
        {
            var toUnload = _chunks.Keys.Where(id => GetChunkDistance(id, center) > unloadRadius).ToList();
            foreach (var id in toUnload)
            {
                if (_chunks.TryGetValue(id, out var chunk))
                {
                    if (chunk.RenderTarget != null)
                    {
                        chunk.RenderTarget.Dispose();
                        chunk.RenderTarget = null;
                    }

                    _chunks.Remove(id);
                }
            }
        }

        // Inactive Entity Management

        public void AddInactiveEntityToChunk(Point chunkId, WorldEntitySnapshot entity)
        {
            if (!_inactiveEntitiesForChunk.ContainsKey(chunkId))
                _inactiveEntitiesForChunk[chunkId] = new List<WorldEntitySnapshot>();

            _inactiveEntitiesForChunk[chunkId].Add(entity);
        }

        public IEnumerable<WorldEntitySnapshot> GetInactiveEntitiesInChunk(Point chunkId)
        {
            if (_inactiveEntitiesForChunk.TryGetValue(chunkId, out var list))
                return list;

            return Enumerable.Empty<WorldEntitySnapshot>();
        }

        public void RemoveInactiveEntityFromChunk(Point chunkId, WorldEntitySnapshot entity)
        {
            if (_inactiveEntitiesForChunk.TryGetValue(chunkId, out var list))
            {
                list.Remove(entity);
                if (list.Count == 0)
                    _inactiveEntitiesForChunk.Remove(chunkId);
            }
        }

        public List<Point> GetAllInactiveEntityChunks()
        {
            return _inactiveEntitiesForChunk.Keys.ToList();
        }

        // Active Entity Management

        public void AddEntityToChunk(Point chunkId, int entityId)
        {
            if (!_activeEntitiesForChunk.ContainsKey(chunkId))
                _activeEntitiesForChunk[chunkId] = new List<int>();

            _activeEntitiesForChunk[chunkId].Add(entityId);
        }

        public void RemoveEntityFromChunk(Point chunkId, int entityId)
        {
            if (_activeEntitiesForChunk.TryGetValue(chunkId, out var list))
            {
                list.Remove(entityId);
                if (list.Count == 0)
                    _activeEntitiesForChunk.Remove(chunkId);
            }
        }

        public IEnumerable<int> GetEntitiesInChunk(Point chunkId)
        {
            if (_activeEntitiesForChunk.TryGetValue(chunkId, out var list))
                return list;

            return Enumerable.Empty<int>();
        }

        public IEnumerable<Point> GetLoadedChunkIds()
        {
            return _chunks.Keys;
        }

        public IEnumerable<Point> GetLoadedChunkIdsAround(Point center, int radius)
        {
            return _chunks.Keys.Where(id => GetChunkDistance(id, center) <= radius);
        }

        // Toroidal Methods

        public Point WrapWorldCoordinates(int x, int y)
        {
            int worldSizeInPixels = WorldSizeInChunks * ChunkSize * PixelSize;
            int wx = (((x * PixelSize) % worldSizeInPixels) + worldSizeInPixels) % worldSizeInPixels;
            int wy = (((y * PixelSize) % worldSizeInPixels) + worldSizeInPixels) % worldSizeInPixels;
            return new Point(wx, wy);
        }

        public Point WrapChunkCoordinates(int x, int y)
        {
            int wx = ((x % WorldSizeInChunks) + WorldSizeInChunks) % WorldSizeInChunks;
            int wy = ((y % WorldSizeInChunks) + WorldSizeInChunks) % WorldSizeInChunks;
            return new Point(wx, wy);
        }

        public int GetChunkDistance(Point a, Point b)
        {
            int dx = Math.Abs(a.X - b.X);
            dx = Math.Min(dx, WorldSizeInChunks - dx);

            int dy = Math.Abs(a.Y - b.Y);
            dy = Math.Min(dy, WorldSizeInChunks - dy);

            return Math.Max(dx, dy);
        }

        public Point GetChunkIdFromWorldCoords(int worldX, int worldY)
        {
            int wx = ((worldX % WorldSizeInPixels) + WorldSizeInPixels) % WorldSizeInPixels;
            int wy = ((worldY % WorldSizeInPixels) + WorldSizeInPixels) % WorldSizeInPixels;

            int chunkX = wx / (ChunkSize * PixelSize);
            int chunkY = wy / (ChunkSize * PixelSize);

            return WrapChunkCoordinates(chunkX, chunkY);
        }

        public Vector2 GetToroidalPosition(Vector2 entityPos, Vector2 referencePos)
        {
            Vector2 delta = entityPos - referencePos;

            if (delta.X > WorldSizeInPixels / 2f) delta.X -= WorldSizeInPixels;
            if (delta.X < -WorldSizeInPixels / 2f) delta.X += WorldSizeInPixels;

            if (delta.Y > WorldSizeInPixels / 2f) delta.Y -= WorldSizeInPixels;
            if (delta.Y < -WorldSizeInPixels / 2f) delta.Y += WorldSizeInPixels;

            return referencePos + delta;
        }

        // Collisions

        public bool IsSolidTile(int tileX, int tileY)
        {
            var worldCoords = WrapWorldCoordinates(tileX, tileY);

            var chunkCoords = WrapChunkCoordinates(
                worldCoords.X / (ChunkSize * PixelSize),
                worldCoords.Y / (ChunkSize * PixelSize));

            var chunk = GetChunk(chunkCoords);
            if (chunk == null || chunk.State == ChunkState.Empty)
                return false;

            int localTileX = ((worldCoords.X / PixelSize) % ChunkSize + ChunkSize) % ChunkSize;
            int localTileY = ((worldCoords.Y / PixelSize) % ChunkSize + ChunkSize) % ChunkSize;

            return chunk.Tiles[localTileX, localTileY] >= 54;
        }

        public void Clear()
        {
            foreach (var chunk in _chunks.Values)
                chunk.RenderTarget?.Dispose();

            _chunks.Clear();
            _activeEntitiesForChunk.Clear();
            _inactiveEntitiesForChunk.Clear();
            GenerationQueue.Clear();
            _modifiedChunks.Clear();
        }
    }
}