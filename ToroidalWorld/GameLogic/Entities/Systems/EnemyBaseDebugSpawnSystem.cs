using System;
using Microsoft.Xna.Framework;
using MonoGame.Extended.ECS.Systems;
using ToroidalWorld.GameEngine;
using ToroidalWorld.GameLogic.Entities;
using ToroidalWorld.GameLogic.Entities.Definitions;
using ToroidalWorld.GameLogic.Map;
using ToroidalWorld.GameLogic.Session;

namespace ToroidalWorld.GameLogic.Entities.Systems
{
    public sealed class EnemyBaseDebugSpawnSystem : UpdateSystem
    {
        private readonly GameSession _session;
        private readonly MapData _map;
        private readonly string _baseName;

        private bool _spawned;

        public EnemyBaseDebugSpawnSystem(GameSession session, MapData map, string baseName)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _map = map ?? throw new ArgumentNullException(nameof(map));
            _baseName = baseName;
        }

        public override void Update(GameTime gameTime)
        {
            if (_spawned)
                return;

            if (_session.World == null || !_session.IsWorldReady || !_session.HasPlayer)
                return;

            if (!_session.TryGetPlayerTransform(out var playerTransform) || playerTransform == null)
                return;

            EnemyBaseDefinition def;

            try
            {
                def = ResourceManager.GetEnemyBaseDefinition(_baseName);
            }
            catch
            {
                return;
            }

            if (!TryFindSpawnNear(playerTransform.Position, def, out var spawnPos))
                return;

            var enemyBase = EntityFactory.CreateEnemyBase(_session.World, _baseName, spawnPos);

            var chunkId = _map.GetChunkIdFromWorldCoords((int)spawnPos.X, (int)spawnPos.Y);
            _map.AddEntityToChunk(chunkId, enemyBase.Id);

            _spawned = true;
        }

        private bool TryFindSpawnNear(Vector2 origin, EnemyBaseDefinition def, out Vector2 spawnPos)
        {
            spawnPos = default;

            float halfW = def.ColliderWidth * 0.5f;
            float halfH = def.ColliderHeight * 0.5f;

            // Candidatos: anillos y 8 direcciones
            float[] radii = { 220f, 320f, 420f, 520f, 650f };
            Vector2[] dirs =
            {
                new Vector2(1f, 0f),
                new Vector2(-1f, 0f),
                new Vector2(0f, 1f),
                new Vector2(0f, -1f),
                Vector2.Normalize(new Vector2(1f, 1f)),
                Vector2.Normalize(new Vector2(1f, -1f)),
                Vector2.Normalize(new Vector2(-1f, 1f)),
                Vector2.Normalize(new Vector2(-1f, -1f))
            };

            for (int r = 0; r < radii.Length; r++)
            {
                for (int d = 0; d < dirs.Length; d++)
                {
                    var candidate = origin + (dirs[d] * radii[r]);

                    var rect = new Rectangle(
                        (int)(candidate.X - halfW),
                        (int)(candidate.Y - halfH),
                        def.ColliderWidth,
                        def.ColliderHeight);

                    if (IntersectsSolid(rect))
                        continue;

                    spawnPos = candidate;
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
    }
}