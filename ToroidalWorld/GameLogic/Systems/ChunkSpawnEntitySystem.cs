using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.ECS.Systems;
using System.Diagnostics;
using System.Linq;
using ToroidalWorld.GameLogic.Entities;
using ToroidalWorld.GameLogic.Entities.Components;
using ToroidalWorld.GameLogic.Map;
using ToroidalWorld.GameLogic.Session;

namespace ToroidalWorld.GameLogic.Systems
{
    public class ChunkSpawnEntitySystem : UpdateSystem
    {
        private readonly OrthographicCamera _camera;
        private readonly MapData _map;
        private readonly GameSession _gameSession;

        public ChunkSpawnEntitySystem(MapData map, OrthographicCamera camera, GameSession gameSession)
        {
            _map = map;
            _camera = camera;
            _gameSession = gameSession;
        }

        public override void Update(GameTime gametime)
        {
            if (_gameSession != null && !_gameSession.IsWorldReady)
                return;

            Point cameraChunkCoords = _map.GetChunkIdFromWorldCoords((int)_camera.Center.X, (int)_camera.Center.Y);

            foreach (Point chunkId in _map.GetAllInactiveEntityChunks())
            {
                if (_map.GetChunkDistance(cameraChunkCoords, chunkId) <= MapData.EntitySpawnDistance)
                {
                    foreach (WorldEntitySnapshot snapshot in _map.GetInactiveEntitiesInChunk(chunkId).ToList())
                    {
                        var oldTransform = snapshot.GetTransform();
                        Vector2 currentPosition = _map.GetToroidalPosition(oldTransform.Position, _camera.Center);

                        switch (snapshot.GetEntityType())
                        {
                            case EntityType.Enemy:
                                var entity = EntityFactory.CreateEnemy(
                                    _gameSession.World,
                                    enemyName: snapshot.GetArchetype(),
                                    position: currentPosition,
                                    rotation: oldTransform.Rotation,
                                    targetEntityId: _gameSession.PlayerEntityId);

                                Debug.WriteLine($"Spawning entity {entity.Id} at chunk {chunkId}");
                                _map.AddEntityToChunk(chunkId, entity.Id);
                                break;

                            case EntityType.EnemyBase:
                                var enemyBase = EntityFactory.CreateEnemyBase(
                                    _gameSession.World,
                                    baseName: snapshot.GetArchetype(),
                                    position: currentPosition,
                                    rotation: oldTransform.Rotation);

                                Debug.WriteLine($"Spawning enemy base {enemyBase.Id} at chunk {chunkId}");
                                _map.AddEntityToChunk(chunkId, enemyBase.Id);
                                break;
                        }

                        _map.RemoveInactiveEntityFromChunk(chunkId, snapshot);
                    }
                }
            }
        }
    }
}