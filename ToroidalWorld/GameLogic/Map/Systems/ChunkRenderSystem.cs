using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.ECS.Systems;
using System;
using ToroidalWorld.Map;

namespace ToroidalWorld.GameLogic.Map.Systems
{
    public class ChunkRenderSystem : DrawSystem
    {
        private MapData _mapData;
        private SpriteBatch _spriteBatch;
        private OrthographicCamera _camera;

        public ChunkRenderSystem(MapData mapData, SpriteBatch spritebatch, OrthographicCamera camera)
        {
            _mapData = mapData;
            _spriteBatch = spritebatch;
            _camera = camera;
        }

        public override void Draw(GameTime gametime)
        {
            int chunkPixels = MapData.ChunkSize * MapData.PixelSize;

            var bounds = _camera.BoundingRectangle;

            int firstChunkX = (int)Math.Floor(bounds.Left / (float)chunkPixels);
            int lastChunkX = (int)Math.Floor(bounds.Right / (float)chunkPixels);

            int firstChunkY = (int)Math.Floor(bounds.Top / (float)chunkPixels);
            int lastChunkY = (int)Math.Floor(bounds.Bottom / (float)chunkPixels);

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: _camera.GetViewMatrix());

            for (int x = firstChunkX; x <= lastChunkX; x++)
            {
                for (int y = firstChunkY; y <= lastChunkY; y++)
                {
                    Point id = _mapData.WrapChunkCoordinates(x, y);

                    Rectangle dest = new Rectangle(
                        x * chunkPixels,
                        y * chunkPixels,
                        chunkPixels,
                        chunkPixels);

                    if (!_mapData.HasChunk(id))
                    {
                        continue;
                    }

                    var chunk = _mapData.GetChunk(id);

                    if (chunk.State == ChunkState.TextureReady && chunk.RenderTarget != null)
                    {
                        _spriteBatch.Draw(chunk.RenderTarget, dest, Color.White);
                    }
                }
            }

            _spriteBatch.End();
        }
    }
}