using System;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using ToroidalWorld.GameLogic.Entities.Components;
using ToroidalWorld.GameLogic.Map;
using ToroidalWorld.GameLogic.Physics.Components;
using ToroidalWorld.GameLogic.Session;

namespace ToroidalWorld.GameLogic.Entities.Systems
{
    /// <summary>
    /// Busca el objetivo más cercano dentro de un rango (en tiles) y escribe el resultado en `TargetingComponent.TargetEntityId`.
    /// El origen de la búsqueda es el `Transform2.Position` de la propia entidad.
    /// </summary>
    public sealed class TargetSearchSystem : EntityUpdateSystem
    {
        private readonly GameSession _session;
        private readonly MapData _map;

        private ComponentMapper<Transform2> _transformMapper;
        private ComponentMapper<TargetingComponent> _targetingMapper;
        private ComponentMapper<EntityCollider> _colliderMapper;
        private ComponentMapper<EntityFlagsComponent> _flagsMapper;

        public TargetSearchSystem(GameSession session, MapData map)
            : base(Aspect.All(typeof(Transform2), typeof(TargetingComponent)))
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _map = map ?? throw new ArgumentNullException(nameof(map));
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _transformMapper = mapperService.GetMapper<Transform2>();
            _targetingMapper = mapperService.GetMapper<TargetingComponent>();
            _colliderMapper = mapperService.GetMapper<EntityCollider>();
            _flagsMapper = mapperService.GetMapper<EntityFlagsComponent>();
        }

        public override void Update(GameTime gameTime)
        {
            foreach (var entityId in ActiveEntities)
            {
                var transform = _transformMapper.Get(entityId);
                var targeting = _targetingMapper.Get(entityId);

                if (transform == null || targeting == null)
                    continue;

                // Si no hay búsqueda configurada, NO tocamos el target actual (permite targets manuales).
                if (targeting.MaxRange <= 0f || targeting.TargetFlagsMask == EntityFlags.None)
                    continue;

                var maxRangeTiles = targeting.MaxRange;
                var maxRangePixels = maxRangeTiles * MapData.PixelSize;
                var maxRangeSq = maxRangePixels * maxRangePixels;

                var originPos = transform.Position;

                // Ruta rápida: si SOLO busca al jugador, devolver directamente el id del player (si está en rango)
                if (targeting.TargetFlagsMask == EntityFlags.Player)
                {
                    targeting.TargetEntityId = ResolvePlayerTarget(originPos, maxRangeSq, entityId);
                    continue;
                }

                var chunkRadius = (int)MathF.Ceiling(maxRangeTiles / MapData.ChunkSize);
                var originChunk = _map.GetChunkIdFromWorldCoords((int)originPos.X, (int)originPos.Y);

                var found = false;
                var bestDistSq = maxRangeSq;
                var bestEntityId = -1;

                for (var dx = -chunkRadius; dx <= chunkRadius; dx++)
                {
                    for (var dy = -chunkRadius; dy <= chunkRadius; dy++)
                    {
                        var neighbor = _map.WrapChunkCoordinates(originChunk.X + dx, originChunk.Y + dy);

                        foreach (var candidateId in _map.GetEntitiesInChunk(neighbor))
                        {
                            if (candidateId == entityId)
                                continue;

                            var collider = _colliderMapper.Get(candidateId);
                            if (collider == null)
                                continue;

                            var candidateFlags = _flagsMapper.Get(candidateId);
                            if (candidateFlags == null)
                                continue;

                            if ((candidateFlags.Flags & targeting.TargetFlagsMask) == 0)
                                continue;

                            var candidateTransform = _transformMapper.Get(candidateId);
                            if (candidateTransform == null)
                                continue;

                            var candidatePos = _map.GetToroidalPosition(candidateTransform.Position, originPos);
                            var distSq = Vector2.DistanceSquared(candidatePos, originPos);

                            if (distSq > maxRangeSq)
                                continue;

                            if (distSq >= bestDistSq)
                                continue;

                            bestDistSq = distSq;
                            bestEntityId = candidateId;
                            found = true;
                        }
                    }
                }

                targeting.TargetEntityId = found ? bestEntityId : -1;
            }
        }

        private int ResolvePlayerTarget(Vector2 originPos, float maxRangeSq, int seekerEntityId)
        {
            if (!_session.HasPlayer)
                return -1;

            if (_session.PlayerEntityId == seekerEntityId)
                return -1;

            if (!_session.TryGetPlayerTransform(out var playerTransform))
                return -1;

            var playerPos = _map.GetToroidalPosition(playerTransform.Position, originPos);
            var distSq = Vector2.DistanceSquared(playerPos, originPos);

            return distSq <= maxRangeSq ? _session.PlayerEntityId : -1;
        }
    }
}