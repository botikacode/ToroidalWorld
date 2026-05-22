using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using MonoGame.Extended.Graphics;
using System.Collections.Generic;
using ToroidalWorld.GameLogic.Entities;
using ToroidalWorld.GameLogic.Entities.Components;
using ToroidalWorld.GameLogic.Map;
using ToroidalWorld.GameLogic.Session;

namespace ToroidalWorld.GameLogic.Systems
{
    public class ChunkActiveEntitySystem : EntityUpdateSystem
    {
        private ComponentMapper<Transform2> _transformMapper;
        private ComponentMapper<WorldPersistentComponent> _persistentMapper;
        private MapData _map;
        private OrthographicCamera _camera;
        private GameSession _gameSession;

        private readonly Dictionary<int, Point> _lastChunkByEntityId = new Dictionary<int, Point>();

        public ChunkActiveEntitySystem(MapData map, OrthographicCamera camera, GameSession gameSession)
            : base(Aspect.All(typeof(Transform2), typeof(WorldPersistentComponent)))
        {
            _map = map;
            _camera = camera;
            _gameSession = gameSession;
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _transformMapper = mapperService.GetMapper<Transform2>();
            _persistentMapper = mapperService.GetMapper<WorldPersistentComponent>();
        }

        public override void Update(GameTime gameTime)
        {
            for (int i = 0; i < ActiveEntities.Count; i++)
            {
                int entityId = ActiveEntities[i];
                var transform = _transformMapper.Get(entityId);

                if (transform == null)
                    continue;

                Point currentChunk = _map.GetChunkIdFromWorldCoords((int)transform.Position.X, (int)transform.Position.Y);
                Point cameraChunk = _map.GetChunkIdFromWorldCoords((int)_camera.Center.X, (int)_camera.Center.Y);

                if (!_lastChunkByEntityId.TryGetValue(entityId, out var lastChunk))
                {
                    _lastChunkByEntityId[entityId] = currentChunk;
                    _map.AddEntityToChunk(currentChunk, entityId);
                    lastChunk = currentChunk;
                }

                if (_map.GetChunkDistance(currentChunk, cameraChunk) > MapData.EntitySpawnDistance)
                {
                    _map.RemoveEntityFromChunk(lastChunk, entityId);

                    var persistent = _persistentMapper.Get(entityId);
                    _map.AddInactiveEntityToChunk(currentChunk, new WorldEntitySnapshot(transform, persistent));

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
    }
}