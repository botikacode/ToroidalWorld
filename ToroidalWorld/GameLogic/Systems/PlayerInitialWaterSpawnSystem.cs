using System;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using ToroidalWorld.GameLogic.Entities.Components;
using ToroidalWorld.GameLogic.Map;
using ToroidalWorld.GameLogic.Physics.Components;
using ToroidalWorld.GameLogic.Session;
using ToroidalWorld.Map;

namespace ToroidalWorld.GameLogic.Systems
{
    public sealed class PlayerInitialWaterSpawnSystem : EntityUpdateSystem
    {
        private readonly GameSession _session;
        private readonly MapData _map;

        private bool _spawnResolved;

        private ComponentMapper<EntityFlagsComponent> _flagsMapper;
        private ComponentMapper<Transform2> _transformMapper;
        private ComponentMapper<TerrainCollider> _terrainColliderMapper;
        private ComponentMapper<MovementState> _movementStateMapper;
        private ComponentMapper<MovementIntent> _movementIntentMapper;

        public PlayerInitialWaterSpawnSystem(GameSession session, MapData map)
            : base(Aspect.All(
                typeof(EntityFlagsComponent),
                typeof(Transform2),
                typeof(TerrainCollider),
                typeof(MovementState),
                typeof(MovementIntent)))
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _map = map ?? throw new ArgumentNullException(nameof(map));
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _flagsMapper = mapperService.GetMapper<EntityFlagsComponent>();
            _transformMapper = mapperService.GetMapper<Transform2>();
            _terrainColliderMapper = mapperService.GetMapper<TerrainCollider>();
            _movementStateMapper = mapperService.GetMapper<MovementState>();
            _movementIntentMapper = mapperService.GetMapper<MovementIntent>();
        }

        public override void Update(GameTime gameTime)
        {
            if (_spawnResolved)
                return;

            if (!TryGetRequiredChunkBounds(out int firstChunkX, out int lastChunkX, out int firstChunkY, out int lastChunkY))
                return;

            if (!AreRequiredChunksDataReady(firstChunkX, lastChunkX, firstChunkY, lastChunkY))
                return;

            for (int i = 0; i < ActiveEntities.Count; i++)
            {
                int entityId = ActiveEntities[i];

                var flags = _flagsMapper.Get(entityId);
                if (flags == null || !flags.Has(EntityFlags.Player))
                    continue;

                var transform = _transformMapper.Get(entityId);
                var collider = _terrainColliderMapper.Get(entityId);

                if (transform == null || collider == null)
                    continue;

                if (!TryFindWaterSpawnPosition(firstChunkX, lastChunkX, firstChunkY, lastChunkY, collider, out var spawnWorldPos))
                    continue;

                ApplySpawn(entityId, transform, spawnWorldPos);
                _spawnResolved = true;
                return;
            }
        }

        private bool TryGetRequiredChunkBounds(out int firstChunkX, out int lastChunkX, out int firstChunkY, out int lastChunkY)
        {
            firstChunkX = lastChunkX = firstChunkY = lastChunkY = 0;

            var camera = _session.Camera;
            if (camera == null)
                return false;

            int chunkPixels = MapData.ChunkSize * MapData.PixelSize;
            int preloadRadius = ChunkStreamingSettings.PreloadRadius;

            ChunkViewport.GetChunkBoundsFromCamera(
                camera,
                chunkPixels,
                preloadRadius,
                out firstChunkX,
                out lastChunkX,
                out firstChunkY,
                out lastChunkY);

            return true;
        }

        private bool AreRequiredChunksDataReady(int firstChunkX, int lastChunkX, int firstChunkY, int lastChunkY)
        {
            for (int x = firstChunkX; x <= lastChunkX; x++)
            {
                for (int y = firstChunkY; y <= lastChunkY; y++)
                {
                    Point id = _map.WrapChunkCoordinates(x, y);
                    var chunk = _map.GetChunk(id);

                    if (chunk == null || chunk.State == ChunkState.Empty)
                        return false;
                }
            }

            return true;
        }

        private bool TryFindWaterSpawnPosition(
            int firstChunkX,
            int lastChunkX,
            int firstChunkY,
            int lastChunkY,
            TerrainCollider collider,
            out Vector2 spawnWorldPos)
        {
            spawnWorldPos = default;

            int pixelSize = MapData.PixelSize;
            float halfW = collider.Width * 0.5f;
            float halfH = collider.Height * 0.5f;

            int firstTileX = firstChunkX * MapData.ChunkSize;
            int lastTileX = ((lastChunkX + 1) * MapData.ChunkSize) - 1;

            int firstTileY = firstChunkY * MapData.ChunkSize;
            int lastTileY = ((lastChunkY + 1) * MapData.ChunkSize) - 1;

            for (int tileY = firstTileY; tileY <= lastTileY; tileY++)
            {
                for (int tileX = firstTileX; tileX <= lastTileX; tileX++)
                {
                    if (_map.IsSolidTile(tileX, tileY))
                        continue;

                    float worldX = (tileX * pixelSize) + (pixelSize * 0.5f);
                    float worldY = (tileY * pixelSize) + (pixelSize * 0.5f);

                    var rect = new Rectangle(
                        (int)(worldX - halfW),
                        (int)(worldY - halfH),
                        collider.Width,
                        collider.Height);

                    if (IntersectsSolid(rect))
                        continue;

                    spawnWorldPos = new Vector2(worldX, worldY);
                    return true;
                }
            }

            return false;
        }

        private bool IntersectsSolid(Rectangle bounds)
        {
            int pixelSize = MapData.PixelSize;

            int minTileX = bounds.Left / pixelSize;
            int maxTileX = (bounds.Right - 1) / pixelSize;
            int minTileY = bounds.Top / pixelSize;
            int maxTileY = (bounds.Bottom - 1) / pixelSize;

            for (int y = minTileY; y <= maxTileY; y++)
            {
                for (int x = minTileX; x <= maxTileX; x++)
                {
                    if (_map.IsSolidTile(x, y))
                        return true;
                }
            }

            return false;
        }

        private void ApplySpawn(int entityId, Transform2 transform, Vector2 worldPos)
        {
            transform.Position = worldPos;

            var state = _movementStateMapper.Get(entityId);
            if (state != null)
            {
                state.OldPosition = worldPos;
                state.ProposedPosition = worldPos;
                state.Velocity = Vector2.Zero;
                state.ProposedRotation = transform.Rotation;
            }

            var intent = _movementIntentMapper.Get(entityId);
            if (intent != null)
            {
                intent.Velocity = Vector2.Zero;
                intent.RotationDelta = 0f;
            }
        }
    }
}