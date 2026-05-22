using Microsoft.Xna.Framework;
using MonoGame.Extended.ECS.Systems;
using System;
using ToroidalWorld.GameEngine;
using ToroidalWorld.GameLogic.Entities;
using ToroidalWorld.GameLogic.Entities.Definitions;
using ToroidalWorld.GameLogic.Map;
using ToroidalWorld.GameLogic.Session;

namespace ToroidalWorld.GameLogic.Entities.Systems
{
    public sealed class StageEnemyBaseSpawnerSystem : UpdateSystem
    {
        private readonly GameSession _session;
        private readonly MapData _map;
        private readonly StageDefinition _stage;

        private readonly Random _random = new Random();

        private bool _spawned;

        public StageEnemyBaseSpawnerSystem(GameSession session, MapData map, string stageId)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _map = map ?? throw new ArgumentNullException(nameof(map));

            _stage = ResourceManager.GetStageDefinition(stageId)
                ?? throw new ArgumentException($"Stage not found: {stageId}", nameof(stageId));
        }

        public override void Update(GameTime gameTime)
        {
            if (_spawned)
                return;

            if (_session.World == null || !_session.IsWorldReady)
                return;

            SpawnEnemyBases();
            _spawned = true;
        }

        private void SpawnEnemyBases()
        {
            if (_stage.EnemyBases == null || _stage.EnemyBases.Count == 0)
                return;

            for (int i = 0; i < _stage.EnemyBases.Count; i++)
            {
                string baseName = _stage.EnemyBases[i];
                if (string.IsNullOrWhiteSpace(baseName))
                    continue;

                if (!IsKnownEnemyBase(baseName))
                    continue;

                Vector2 pos = GetRandomWorldPosition();
                float rotation = (float)(_random.NextDouble() * (Math.PI * 2.0));

                EntityFactory.CreateEnemyBase(_session.World, baseName, pos, rotation);
            }
        }

        private bool IsKnownEnemyBase(string baseName)
        {
            try
            {
                _ = ResourceManager.GetEnemyBaseDefinition(baseName);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private Vector2 GetRandomWorldPosition()
        {
            int worldSize = _map.WorldSizeInPixels;

            float x = _random.Next(0, worldSize);
            float y = _random.Next(0, worldSize);

            return new Vector2(x, y);
        }
    }
}