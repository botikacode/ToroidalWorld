using System;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.ECS.Systems;

namespace ToroidalWorld.GameLogic.Map.Systems;

public class VisibleChunkRequestSystem : UpdateSystem
{
    private readonly MapData _mapData;
    private readonly OrthographicCamera _camera;

    public VisibleChunkRequestSystem(MapData mapData, OrthographicCamera camera)
    {
        _mapData = mapData;
        _camera = camera;
    }

    public override void Update(GameTime gametime)
    {
        int chunkPixels = MapData.ChunkSize * MapData.PixelSize;
        int preloadRadius = ChunkStreamingSettings.PreloadRadius;

        ChunkViewport.GetChunkBoundsFromCamera(
            _camera,
            chunkPixels,
            preloadRadius,
            out int firstChunkX,
            out int lastChunkX,
            out int firstChunkY,
            out int lastChunkY);

        for (int x = firstChunkX; x <= lastChunkX; x++)
        {
            for (int y = firstChunkY; y <= lastChunkY; y++)
            {
                Point id = _mapData.WrapChunkCoordinates(x, y);

                if (_mapData.HasChunk(id))
                    continue;

                if (_mapData.GenerationQueue.Contains(id))
                    continue;

                _mapData.GenerationQueue.Enqueue(id);
            }
        }

        int centerChunkX = (int)Math.Floor(_camera.Center.X / (float)chunkPixels);
        int centerChunkY = (int)Math.Floor(_camera.Center.Y / (float)chunkPixels);
        Point center = _mapData.WrapChunkCoordinates(centerChunkX, centerChunkY);

        // Dynamic unload radius: ensures that we do NOT unload anything within the requested area (viewport + preload).
        // Use distance to the corners (in Chebyshev metric) because MapData.GetChunkDistance returns Max(dx, dy) with toroidal wrap.
        Point c0 = _mapData.WrapChunkCoordinates(firstChunkX, firstChunkY);
        Point c1 = _mapData.WrapChunkCoordinates(firstChunkX, lastChunkY);
        Point c2 = _mapData.WrapChunkCoordinates(lastChunkX, firstChunkY);
        Point c3 = _mapData.WrapChunkCoordinates(lastChunkX, lastChunkY);

        int requiredRadius = 0;
        requiredRadius = Math.Max(requiredRadius, _mapData.GetChunkDistance(center, c0));
        requiredRadius = Math.Max(requiredRadius, _mapData.GetChunkDistance(center, c1));
        requiredRadius = Math.Max(requiredRadius, _mapData.GetChunkDistance(center, c2));
        requiredRadius = Math.Max(requiredRadius, _mapData.GetChunkDistance(center, c3));

        int unloadRadius = requiredRadius + ChunkStreamingSettings.UnloadExtraRadius;

        _mapData.UnloadFarChunks(center, unloadRadius);
    }
}