using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using ToroidalWorld.GameLogic.Entities.Components;
using ToroidalWorld.GameLogic.Map;
using ToroidalWorld.GameLogic.Physics;
using ToroidalWorld.GameLogic.Physics.Components;
using ToroidalWorld.GameLogic.Session;

namespace ToroidalWorld.GameLogic.Entities.Systems
{
    public sealed class IcebreakerSystem : EntityUpdateSystem
    {
        private readonly GameSession _session;
        private readonly MapData _map;
        private readonly DerivedCollisionEvents _derived;

        private readonly HashSet<int> _hitEntityIds = new HashSet<int>();

        private ComponentMapper<Transform2> _transformMapper;
        private ComponentMapper<IcebreakerComponent> _icebreakerMapper;
        private ComponentMapper<MovementIntent> _intentMapper;
        private ComponentMapper<MovementState> _stateMapper;
        private ComponentMapper<TerrainCollider> _terrainColliderMapper;
        private ComponentMapper<StatMultipliersComponent> _multipliersMapper;

        private const float MinIntentSpeedSq = 0.01f;

        // Para no dejar “islas” pegadas al casco cuando giras
        private const int BackTiles = 1;

        // Para evitar que el rompehielos “muerda” de lado al rozar
        private const float ForwardContactEpsilonPx = 1f;

        public IcebreakerSystem(GameSession session, MapData map, DerivedCollisionEvents derived)
            : base(Aspect.All(typeof(IcebreakerComponent), typeof(MovementIntent), typeof(MovementState), typeof(TerrainCollider), typeof(Transform2)))
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _map = map ?? throw new ArgumentNullException(nameof(map));
            _derived = derived ?? throw new ArgumentNullException(nameof(derived));
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _transformMapper = mapperService.GetMapper<Transform2>();
            _icebreakerMapper = mapperService.GetMapper<IcebreakerComponent>();
            _intentMapper = mapperService.GetMapper<MovementIntent>();
            _stateMapper = mapperService.GetMapper<MovementState>();
            _terrainColliderMapper = mapperService.GetMapper<TerrainCollider>();
            _multipliersMapper = mapperService.GetMapper<StatMultipliersComponent>();
        }

        public override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            CollectHits();

            for (int i = 0; i < ActiveEntities.Count; i++)
            {
                int entityId = ActiveEntities[i];

                var transform = _transformMapper.Get(entityId);
                var ice = _icebreakerMapper.Get(entityId);
                var intent = _intentMapper.Get(entityId);
                var state = _stateMapper.Get(entityId);
                var terrainCollider = _terrainColliderMapper.Get(entityId);

                if (transform == null || ice == null || intent == null || state == null || terrainCollider == null)
                    continue;

                if (!_hitEntityIds.Contains(entityId))
                    continue;

                if (intent.Velocity.LengthSquared() < MinIntentSpeedSq)
                    continue;

                ice.RemainingSeconds -= dt;
                if (ice.RemainingSeconds > 0f)
                    continue;

                var mult = _multipliersMapper.Get(entityId);

                float rateMult = mult?.IcebreakerRateMultiplier ?? 1f;
                if (rateMult <= 0f)
                    rateMult = 1f;

                float strengthMult = mult?.IcebreakerStrengthMultiplier ?? 1f;
                if (strengthMult < 0f)
                    strengthMult = 0f;

                float effectiveCooldown = ice.CooldownSeconds / rateMult;
                if (effectiveCooldown < 0.01f)
                    effectiveCooldown = 0.01f;

                int strength = (int)MathF.Round(ice.DeltaPerHit * strengthMult);
                if (strength < 1)
                    strength = 1;

                // FAILSAFE anti-atoro:
                // siempre que haya colisión (aunque sea lateral) limpia cualquier sólido dentro del collider,
                // expandido por HalfWidthTiles.
                ClearTerrainInsideCollider(
                    entityWorldPos: state.ProposedPosition,
                    rotation: state.ProposedRotation,
                    collider: terrainCollider,
                    extraHalfWidthTiles: ice.HalfWidthTiles);

                // Cuña estética: solo si el contacto es frontal (evita “socavones” al rozar)
                if (HasForwardContact(entityId, transform, state, terrainCollider))
                {
                    BreakWedgeAtBow(
                        entityWorldPos: state.ProposedPosition,
                        rotation: transform.Rotation,
                        collider: terrainCollider,
                        forwardTiles: ice.ForwardTiles,
                        extraHalfWidthTiles: ice.HalfWidthTiles,
                        deltaPerHit: strength);

                    BreakWedgeAtBow(
                        entityWorldPos: state.ProposedPosition,
                        rotation: state.ProposedRotation,
                        collider: terrainCollider,
                        forwardTiles: ice.ForwardTiles,
                        extraHalfWidthTiles: ice.HalfWidthTiles,
                        deltaPerHit: strength);
                }

                ice.RemainingSeconds = effectiveCooldown;
            }
        }

        private void CollectHits()
        {
            _hitEntityIds.Clear();

            for (int i = 0; i < _derived.TerrainCollisions.Count; i++)
            {
                var evt = _derived.TerrainCollisions[i];
                _hitEntityIds.Add(evt.EntityId);
            }
        }

        private bool HasForwardContact(int entityId, Transform2 transform, MovementState state, TerrainCollider collider)
        {
            int pixelSize = MapData.PixelSize;

            float halfWpx = collider.Width * 0.5f;
            float halfHpx = collider.Height * 0.5f;

            float maxLateralPx = halfWpx + pixelSize;

            bool anyForward = false;

            for (int i = 0; i < _derived.TerrainCollisions.Count; i++)
            {
                var evt = _derived.TerrainCollisions[i];
                if (evt.EntityId != entityId)
                    continue;

                Vector2 hitWorld = new Vector2(
                    (evt.WorldTile.X * pixelSize) + (pixelSize * 0.5f),
                    (evt.WorldTile.Y * pixelSize) + (pixelSize * 0.5f));

                if (IsForwardContact(hitWorld, state.ProposedPosition, transform.Rotation, halfHpx, maxLateralPx))
                    return true;

                if (IsForwardContact(hitWorld, state.ProposedPosition, state.ProposedRotation, halfHpx, maxLateralPx))
                    anyForward = true;
            }

            return anyForward;
        }

        private static bool IsForwardContact(Vector2 hitWorld, Vector2 entityPos, float rotation, float halfHpx, float maxLateralPx)
        {
            Vector2 forward = new Vector2(
                (float)Math.Cos(rotation - MathHelper.PiOver2),
                (float)Math.Sin(rotation - MathHelper.PiOver2));

            Vector2 right = new Vector2(-forward.Y, forward.X);

            Vector2 bow = entityPos + (forward * halfHpx);
            Vector2 d = hitWorld - bow;

            float forwardProj = Vector2.Dot(d, forward);
            float lateralProj = MathF.Abs(Vector2.Dot(d, right));

            return forwardProj >= -ForwardContactEpsilonPx && lateralProj <= maxLateralPx;
        }

        private void BreakWedgeAtBow(
            Vector2 entityWorldPos,
            float rotation,
            TerrainCollider collider,
            int forwardTiles,
            int extraHalfWidthTiles,
            int deltaPerHit)
        {
            if (forwardTiles < 0) forwardTiles = 0;
            if (extraHalfWidthTiles < 0) extraHalfWidthTiles = 0;

            int pixelSize = MapData.PixelSize;

            float halfWpx = collider.Width * 0.5f;
            float halfHpx = collider.Height * 0.5f;

            int colliderHalfWidthTiles = (int)MathF.Ceiling(halfWpx / pixelSize);

            // Base ancha hacia el barco, punta estrecha hacia delante.
            int baseHalfWidthTiles = colliderHalfWidthTiles + extraHalfWidthTiles;
            int tipHalfWidthTiles = Math.Min(2, baseHalfWidthTiles);

            Vector2 forward = new Vector2(
                (float)Math.Cos(rotation - MathHelper.PiOver2),
                (float)Math.Sin(rotation - MathHelper.PiOver2));

            Vector2 right = new Vector2(-forward.Y, forward.X);

            Vector2 bow = entityWorldPos + (forward * halfHpx);

            int delta = -Math.Abs(deltaPerHit);

            int totalSteps = forwardTiles + BackTiles;
            if (totalSteps < 1)
                totalSteps = 1;

            for (int f = -BackTiles; f <= forwardTiles; f++)
            {
                float progress = (f + BackTiles) / (float)totalSteps;
                progress = MathHelper.Clamp(progress, 0f, 1f);

                // base ancha en progress=0, punta estrecha en progress=1
                int halfWidthTiles = (int)MathF.Round(MathHelper.Lerp(baseHalfWidthTiles, tipHalfWidthTiles, progress));

                Vector2 fOffset = forward * (f * pixelSize);

                for (int w = -halfWidthTiles; w <= halfWidthTiles; w++)
                {
                    Vector2 sampleWorld = bow + fOffset + (right * (w * pixelSize));

                    int tx = (int)MathF.Floor(sampleWorld.X / pixelSize);
                    int ty = (int)MathF.Floor(sampleWorld.Y / pixelSize);

                    if (!_map.IsSolidTile(tx, ty))
                        continue;

                    _map.ApplyDeltaToTile(tx, ty, delta);
                }
            }
        }

        private void ClearTerrainInsideCollider(
            Vector2 entityWorldPos,
            float rotation,
            TerrainCollider collider,
            int extraHalfWidthTiles)
        {
            if (extraHalfWidthTiles < 0)
                extraHalfWidthTiles = 0;

            int pixelSize = MapData.PixelSize;

            float halfWpx = collider.Width * 0.5f;
            float halfHpx = collider.Height * 0.5f;

            float extraPx = extraHalfWidthTiles * pixelSize;

            Vector2 forward = new Vector2(
                (float)Math.Cos(rotation - MathHelper.PiOver2),
                (float)Math.Sin(rotation - MathHelper.PiOver2));

            Vector2 right = new Vector2(-forward.Y, forward.X);

            // Esquinas del OBB para obtener AABB de barrido en tiles
            Vector2 c0 = entityWorldPos + (right * halfWpx) + (forward * halfHpx);
            Vector2 c1 = entityWorldPos + (right * halfWpx) - (forward * halfHpx);
            Vector2 c2 = entityWorldPos - (right * halfWpx) + (forward * halfHpx);
            Vector2 c3 = entityWorldPos - (right * halfWpx) - (forward * halfHpx);

            float minX = MathF.Min(MathF.Min(c0.X, c1.X), MathF.Min(c2.X, c3.X)) - extraPx;
            float maxX = MathF.Max(MathF.Max(c0.X, c1.X), MathF.Max(c2.X, c3.X)) + extraPx;
            float minY = MathF.Min(MathF.Min(c0.Y, c1.Y), MathF.Min(c2.Y, c3.Y)) - extraPx;
            float maxY = MathF.Max(MathF.Max(c0.Y, c1.Y), MathF.Max(c2.Y, c3.Y)) + extraPx;

            int minTileX = (int)MathF.Floor(minX / pixelSize);
            int maxTileX = (int)MathF.Floor(maxX / pixelSize);
            int minTileY = (int)MathF.Floor(minY / pixelSize);
            int maxTileY = (int)MathF.Floor(maxY / pixelSize);

            // Margen para compensar redondeos al girar
            float marginPx = (pixelSize * 0.75f) + extraPx;

            // Forzar a agua si está dentro (evita “islas” residuales)
            int forceDelta = -9999;

            for (int ty = minTileY; ty <= maxTileY; ty++)
            {
                for (int tx = minTileX; tx <= maxTileX; tx++)
                {
                    if (!_map.IsSolidTile(tx, ty))
                        continue;

                    Vector2 tileCenter = new Vector2(
                        (tx * pixelSize) + (pixelSize * 0.5f),
                        (ty * pixelSize) + (pixelSize * 0.5f));

                    Vector2 d = tileCenter - entityWorldPos;

                    float localX = MathF.Abs(Vector2.Dot(d, right));
                    float localY = MathF.Abs(Vector2.Dot(d, forward));

                    if (localX <= halfWpx + marginPx && localY <= halfHpx + marginPx)
                        _map.ApplyDeltaToTile(tx, ty, forceDelta);
                }
            }
        }
    }
}