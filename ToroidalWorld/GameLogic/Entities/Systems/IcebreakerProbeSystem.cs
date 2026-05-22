using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using ToroidalWorld.GameEngine;
using ToroidalWorld.GameLogic.Entities.Components;
using ToroidalWorld.GameLogic.Map;
using ToroidalWorld.GameLogic.Physics.Components;

namespace ToroidalWorld.GameLogic.Entities.Systems
{
    public sealed class IcebreakerProbeSystem : EntityProcessingSystem
    {
        private const float ProbeTipWidthFactor = 0.25f;

        private readonly MapData _map;

        private readonly Dictionary<int, SoundEffectInstance> _loopByEntityId = new Dictionary<int, SoundEffectInstance>(16);

        private ComponentMapper<Transform2> _transformMapper;
        private ComponentMapper<MovementState> _stateMapper;
        private ComponentMapper<TerrainCollider> _colliderMapper;
        private ComponentMapper<IcebreakerComponent> _iceMapper;
        private ComponentMapper<StatMultipliersComponent> _multMapper;
        private ComponentMapper<MoveStatsComponent> _moveStatsMapper;

        public IcebreakerProbeSystem(MapData map)
            : base(Aspect.All(typeof(Transform2), typeof(MovementState), typeof(TerrainCollider), typeof(IcebreakerComponent), typeof(MoveStatsComponent)))
        {
            _map = map ?? throw new ArgumentNullException(nameof(map));
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _transformMapper = mapperService.GetMapper<Transform2>();
            _stateMapper = mapperService.GetMapper<MovementState>();
            _colliderMapper = mapperService.GetMapper<TerrainCollider>();
            _iceMapper = mapperService.GetMapper<IcebreakerComponent>();
            _multMapper = mapperService.GetMapper<StatMultipliersComponent>();
            _moveStatsMapper = mapperService.GetMapper<MoveStatsComponent>();
        }

        public override void Process(GameTime gameTime, int entityId)
        {
            var transform = _transformMapper.Get(entityId);
            var state = _stateMapper.Get(entityId);
            var collider = _colliderMapper.Get(entityId);
            var ice = _iceMapper.Get(entityId);
            var moveStats = _moveStatsMapper.Get(entityId);

            if (transform == null || state == null || collider == null || ice == null || moveStats == null)
                return;

            if (!ice.ProbeEnabled)
            {
                SetBreakingLoop(entityId, ice, isBreaking: false);
                return;
            }

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (dt <= 0f)
                return;

            if (state.Velocity.LengthSquared() < 0.0001f)
            {
                SetBreakingLoop(entityId, ice, isBreaking: false);
                return;
            }

            var mult = _multMapper.Get(entityId);

            float rateMult = mult?.IcebreakerRateMultiplier ?? 1f;
            if (rateMult <= 0f)
                rateMult = 1f;

            float strengthMult = mult?.IcebreakerStrengthMultiplier ?? 1f;
            if (strengthMult < 0f)
                strengthMult = 0f;

            int pixelSize = MapData.PixelSize;

            float halfWpx = collider.Width * 0.5f;
            float halfHpx = collider.Height * 0.5f;

            float extraPx = Math.Max(0, ice.ProbeHalfWidthTiles) * pixelSize;
            float baseHalfWidthPx = halfWpx + extraPx;

            float probeLengthPx = Math.Max(0, ice.ProbeForwardTiles) * pixelSize;
            if (probeLengthPx <= 0f)
            {
                SetBreakingLoop(entityId, ice, isBreaking: false);
                return;
            }

            float tipHalfWidthPx = baseHalfWidthPx * ProbeTipWidthFactor;
            tipHalfWidthPx = MathF.Max(tipHalfWidthPx, pixelSize * 0.5f);
            tipHalfWidthPx = MathF.Min(tipHalfWidthPx, baseHalfWidthPx);

            Vector2 moveDir = Vector2.Normalize(state.Velocity);

            Vector2 forward = new Vector2(
                (float)Math.Cos(state.ProposedRotation - MathHelper.PiOver2),
                (float)Math.Sin(state.ProposedRotation - MathHelper.PiOver2));

            if (Vector2.Dot(forward, moveDir) < 0f)
                forward = -forward;

            Vector2 right = new Vector2(-forward.Y, forward.X);

            Vector2 bow = transform.Position + (forward * halfHpx);
            Vector2 tip = bow + (forward * probeLengthPx);

            // AABB del trapecio (cuña): ancho grande en bow, ancho pequeño en tip.
            Vector2 v0 = bow + (right * baseHalfWidthPx);
            Vector2 v1 = bow - (right * baseHalfWidthPx);
            Vector2 v2 = tip - (right * tipHalfWidthPx);
            Vector2 v3 = tip + (right * tipHalfWidthPx);

            float minX = MathF.Min(MathF.Min(v0.X, v1.X), MathF.Min(v2.X, v3.X));
            float maxX = MathF.Max(MathF.Max(v0.X, v1.X), MathF.Max(v2.X, v3.X));
            float minY = MathF.Min(MathF.Min(v0.Y, v1.Y), MathF.Min(v2.Y, v3.Y));
            float maxY = MathF.Max(MathF.Max(v0.Y, v1.Y), MathF.Max(v2.Y, v3.Y));

            int minTileX = (int)MathF.Floor(minX / pixelSize);
            int maxTileX = (int)MathF.Floor(maxX / pixelSize);
            int minTileY = (int)MathF.Floor(minY / pixelSize);
            int maxTileY = (int)MathF.Floor(maxY / pixelSize);

            float invLen = 1f / MathF.Max(1f, probeLengthPx);

            int solids = 0;

            for (int ty = minTileY; ty <= maxTileY; ty++)
            {
                for (int tx = minTileX; tx <= maxTileX; tx++)
                {
                    if (!_map.IsSolidTile(tx, ty))
                        continue;

                    Vector2 tileCenter = new Vector2(
                        (tx * pixelSize) + (pixelSize * 0.5f),
                        (ty * pixelSize) + (pixelSize * 0.5f));

                    Vector2 d = tileCenter - bow;

                    float localF = Vector2.Dot(d, forward);
                    if (localF < 0f || localF > probeLengthPx)
                        continue;

                    float u = MathHelper.Clamp(localF * invLen, 0f, 1f);
                    float allowedHalfWidth = MathHelper.Lerp(baseHalfWidthPx, tipHalfWidthPx, u);

                    float localR = MathF.Abs(Vector2.Dot(d, right));
                    if (localR > allowedHalfWidth)
                        continue;

                    solids++;
                }
            }

            bool isBreaking = solids > 0;
            SetBreakingLoop(entityId, ice, isBreaking);

            // CAP de velocidad (no bloquea aceleración)
            if (isBreaking && moveStats.MaxSpeed > 0f)
            {
                float resistanceScale = 1f / Math.Max(1f, strengthMult);

                float effectiveSlowdown = ice.SlowdownPerSolidTile * resistanceScale;

                float capMult = 1f - (solids * effectiveSlowdown);
                capMult = MathHelper.Clamp(capMult, ice.MinSpeedMultiplier, 1f);

                float maxSpeedInIce = moveStats.MaxSpeed * capMult;

                float speed = state.Velocity.Length();
                if (speed > maxSpeedInIce && speed > 0f)
                    state.Velocity = (state.Velocity / speed) * maxSpeedInIce;

                state.ProposedPosition = state.OldPosition + (state.Velocity * dt);
            }

            // Rotura anticipada (cooldown propio)
            ice.ProbeRemainingSeconds -= dt;
            if (ice.ProbeRemainingSeconds > 0f)
                return;

            float effectiveCooldown = ice.ProbeCooldownSeconds / rateMult;
            if (effectiveCooldown < 0.005f)
                effectiveCooldown = 0.005f;

            int breakStrength = (int)MathF.Round(ice.ProbeDeltaPerHit * strengthMult);
            if (breakStrength < 1)
                breakStrength = 1;

            int delta = -Math.Abs(breakStrength);

            for (int ty = minTileY; ty <= maxTileY; ty++)
            {
                for (int tx = minTileX; tx <= maxTileX; tx++)
                {
                    if (!_map.IsSolidTile(tx, ty))
                        continue;

                    Vector2 tileCenter = new Vector2(
                        (tx * pixelSize) + (pixelSize * 0.5f),
                        (ty * pixelSize) + (pixelSize * 0.5f));

                    Vector2 d = tileCenter - bow;

                    float localF = Vector2.Dot(d, forward);
                    if (localF < 0f || localF > probeLengthPx)
                        continue;

                    float u = MathHelper.Clamp(localF * invLen, 0f, 1f);
                    float allowedHalfWidth = MathHelper.Lerp(baseHalfWidthPx, tipHalfWidthPx, u);

                    float localR = MathF.Abs(Vector2.Dot(d, right));
                    if (localR > allowedHalfWidth)
                        continue;

                    _map.ApplyDeltaToTile(tx, ty, delta);
                }
            }

            ice.ProbeRemainingSeconds = effectiveCooldown;
        }

        private void SetBreakingLoop(int entityId, IcebreakerComponent ice, bool isBreaking)
        {
            if (string.IsNullOrWhiteSpace(ice.BreakingLoopSfxKey))
                return;

            if (isBreaking)
            {
                if (!_loopByEntityId.TryGetValue(entityId, out var instance) || instance == null)
                {
                    instance = AudioManager.CreateSoundEffectInstance(ice.BreakingLoopSfxKey, isLooped: true);
                    if (instance != null)
                        _loopByEntityId[entityId] = instance;
                }

                if (instance != null && instance.State != SoundState.Playing)
                    instance.Play();

                return;
            }

            if (_loopByEntityId.TryGetValue(entityId, out var existing) && existing != null && existing.State == SoundState.Playing)
                existing.Stop();
        }
    }
}