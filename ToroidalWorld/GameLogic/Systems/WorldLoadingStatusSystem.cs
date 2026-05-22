using System;
using Microsoft.Xna.Framework;
using MonoGame.Extended.ECS.Systems;
using ToroidalWorld.GameLogic.Map;
using ToroidalWorld.GameLogic.Session;
using ToroidalWorld.Map;

namespace ToroidalWorld.GameLogic.Systems
{
    public sealed class WorldLoadingStatusSystem : UpdateSystem
    {
        private readonly GameSession _session;
        private readonly MapData _map;

        public WorldLoadingStatusSystem(GameSession session, MapData map)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _map = map ?? throw new ArgumentNullException(nameof(map));
        }

        public override void Update(GameTime gameTime)
        {
            if (_session.IsWorldReady)
                return;

            _session.WorldReadyChunks = 0;
            _session.WorldTotalChunks = 0;

            var camera = _session.Camera;
            if (camera == null)
                return;

            int chunkPixels = MapData.ChunkSize * MapData.PixelSize;

            ChunkViewport.GetChunkBoundsFromCamera(
                camera,
                chunkPixels,
                ChunkStreamingSettings.PreloadRadius,
                out int firstChunkX,
                out int lastChunkX,
                out int firstChunkY,
                out int lastChunkY);

            for (int x = firstChunkX; x <= lastChunkX; x++)
            {
                for (int y = firstChunkY; y <= lastChunkY; y++)
                {
                    _session.WorldTotalChunks++;

                    Point id = _map.WrapChunkCoordinates(x, y);
                    var chunk = _map.GetChunk(id);

                    if (chunk != null && chunk.State == ChunkState.TextureReady)
                        _session.WorldReadyChunks++;
                }
            }

            _session.IsWorldReady =
                _session.WorldTotalChunks > 0 &&
                _session.WorldReadyChunks >= _session.WorldTotalChunks;
        }
    }
}