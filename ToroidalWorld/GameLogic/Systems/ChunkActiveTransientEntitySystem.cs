using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using MonoGame.Extended.Graphics;
using System.Collections.Generic;
using ToroidalWorld.GameLogic.Entities.Components;
using ToroidalWorld.GameLogic.Map;
using ToroidalWorld.GameLogic.Physics.Components;
using ToroidalWorld.GameLogic.Session;

namespace ToroidalWorld.GameLogic.Systems
{
    public sealed class ChunkActiveTransientEntitySystem : EntityUpdateSystem
    {
        private ComponentMapper<Transform2> _transformMapper;

        private readonly MapData _map;
        private readonly OrthographicCamera _camera;
        private readonly GameSession _gameSession;

        private readonly Dictionary<int, Point> _lastChunkByEntityId = new Dictionary<int, Point>();

        public ChunkActiveTransientEntitySystem(MapData map, OrthographicCamera camera, GameSession gameSession)
            : base(Aspect.All(typeof(Transform2), typeof(EntityCollider))
                .Exclude(typeof(WorldPersistentComponent)))
        {
            _map = map;
            _camera = camera;
            _gameSession = gameSession;
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _transformMapper = mapperService.GetMapper<Transform2>();
        }

        public override void Update(GameTime gameTime)
        {
            for (int i = 0; i < ActiveEntities.Count; i++)
            {
                int entityId = ActiveEntities[i];

                if (_gameSession != null && entityId == _gameSession.PlayerEntityId)
                    continue;

                var transform = _transformMapper.Get(entityId);

                if (transform == null)
                    continue;

                Point currentChunk = _map.GetChunkIdFromWorldCoords((int)transform.Position.X, (int)transform.Position.Y);
                Point cameraChunk = _map.GetChunkIdFromWorldCoords((int)_camera.Center.X, (int)_camera.Center.Y);

                if (!_lastChunkByEntityId.TryGetValue(entityId, out var lastChunk) || !IsEntityRegisteredInChunk(lastChunk, entityId))
                {
                    _lastChunkByEntityId[entityId] = currentChunk;
                    _map.AddEntityToChunk(currentChunk, entityId);
                    lastChunk = currentChunk;
                }

                if (_map.GetChunkDistance(currentChunk, cameraChunk) > MapData.EntitySpawnDistance)
                {
                    _map.RemoveEntityFromChunk(lastChunk, entityId);

                    _lastChunkByEntityId.Remove(entityId);
                    _gameSession.DestroyEntityById(entityId);
                    continue;
                }

                if (currentChunk != lastChunk)
                {
                    _map.RemoveEntityFromChunk(lastChunk, entityId);
                    _map.AddEntityToChunk(currentChunk, entityId);
                    _lastChunkByEntityId[entityId] = currentChunk;
                }
            }
        }

        private bool IsEntityRegisteredInChunk(Point chunkId, int entityId)
        {
            foreach (int id in _map.GetEntitiesInChunk(chunkId))
            {
                if (id == entityId)
                    return true;
            }

            return false;
        }
    }
}