using Microsoft.Xna.Framework;
using MonoGame.Extended.ECS.Systems;
using System;
using System.Collections.Generic;
using ToroidalWorld.GameEngine;
using ToroidalWorld.GameLogic.Entities.Definitions;
using ToroidalWorld.GameLogic.Session;

namespace ToroidalWorld.GameLogic.Entities.Systems
{
    public sealed class StageSpawnerSystem : UpdateSystem
    {
        private readonly GameSession _session;
        private readonly StageDefinition _stage;
        private readonly Random _random = new Random();

        private float _spawnTimer;

        public StageSpawnerSystem(GameSession session, string stageId)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _stage = ResourceManager.GetStageDefinition(stageId) ?? throw new ArgumentException($"Stage not found: {stageId}", nameof(stageId));

            _session.Stats.Stage.StageId = stageId;
        }

        public override void Update(GameTime gameTime)
        {
            if (!_session.IsWorldReady)
                return;

            if (!_session.TryGetPlayerTransform(out var playerTransform))
                return;

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            var state = _session.Stats.Stage;

            state.TimeSeconds += dt;
            state.WaveTimeSeconds += dt;
            _spawnTimer += dt;

            if (state.TargetEnemiesThisWave <= 0)
                StartNewWave(state);

            while (state.SpawnedEnemiesThisWave < state.TargetEnemiesThisWave && _spawnTimer >= _stage.SpawnIntervalSeconds)
            {
                _spawnTimer -= _stage.SpawnIntervalSeconds;

                SpawnOne(playerTransform.Position, state.WaveIndex);

                state.SpawnedEnemiesThisWave++;
            }

            if (state.WaveTimeSeconds >= _stage.WaveDurationSeconds)
                StartNewWave(state);
        }

        private void StartNewWave(StageRuntimeState state)
        {
            state.WaveIndex++;
            state.WaveTimeSeconds = 0f;
            _spawnTimer = 0f;
            state.SpawnedEnemiesThisWave = 0;

            state.TargetEnemiesThisWave = ComputeEnemiesThisWave(state.WaveIndex);
            if (state.TargetEnemiesThisWave < 1)
                state.TargetEnemiesThisWave = 1;
        }

        private int ComputeEnemiesThisWave(int waveIndex)
        {
            float raw = _stage.BaseEnemiesPerWave * System.MathF.Pow(_stage.Scaling.EnemyCountMultiplier, waveIndex);
            int value = (int)System.MathF.Round(raw);

            if (_stage.Scaling.MaxEnemiesPerWave > 0 && value > _stage.Scaling.MaxEnemiesPerWave)
                value = _stage.Scaling.MaxEnemiesPerWave;

            return value;
        }

        private void SpawnOne(Vector2 playerPos, int waveIndex)
        {
            string enemyName = PickEnemyNameForWave(waveIndex);
            if (string.IsNullOrWhiteSpace(enemyName))
                return;

            Vector2 spawnPos = GetRandomPointInRing(playerPos, _stage.SpawnRing.MinRadius, _stage.SpawnRing.MaxRadius);

            var scaling = new EnemyScaling
            {
                HealthMultiplier = System.MathF.Pow(_stage.Scaling.HealthMultiplier, waveIndex),
                SpeedMultiplier = System.MathF.Pow(_stage.Scaling.SpeedMultiplier, waveIndex),
                DamageMultiplier = System.MathF.Pow(_stage.Scaling.DamageMultiplier, waveIndex),
            };

            EntityFactory.CreateEnemy(_session.World, enemyName, spawnPos, rotation: 0f, targetEntityId: _session.PlayerEntityId, scaling: scaling);
        }

        private Vector2 GetRandomPointInRing(Vector2 center, float minRadius, float maxRadius)
        {
            if (minRadius < 0f)
                minRadius = 0f;

            if (maxRadius < minRadius)
                maxRadius = minRadius;

            double angle = _random.NextDouble() * System.Math.PI * 2.0;
            float radius = minRadius + ((float)_random.NextDouble() * (maxRadius - minRadius));

            return center + new Vector2(
                (float)System.Math.Cos(angle) * radius,
                (float)System.Math.Sin(angle) * radius);
        }

        private string PickEnemyNameForWave(int waveIndex)
        {
            var active = GetActiveParticipants(waveIndex);
            if (active.Count == 0)
                return null;

            float total = 0f;
            for (int i = 0; i < active.Count; i++)
                total += System.MathF.Max(0f, active[i].Weight);

            if (total <= 0f)
                return active[0].EnemyName;

            float roll = (float)_random.NextDouble() * total;

            for (int i = 0; i < active.Count; i++)
            {
                float w = System.MathF.Max(0f, active[i].Weight);
                roll -= w;
                if (roll <= 0f)
                    return active[i].EnemyName;
            }

            return active[active.Count - 1].EnemyName;
        }

        private List<StageParticipantDefinition> GetActiveParticipants(int waveIndex)
        {
            var list = new List<StageParticipantDefinition>();

            foreach (var p in _stage.Participants)
            {
                if (p == null || string.IsNullOrWhiteSpace(p.EnemyName))
                    continue;

                if (waveIndex < p.StartWave)
                    continue;

                if (p.EndWave >= 0 && waveIndex > p.EndWave)
                    continue;

                list.Add(p);
            }

            return list;
        }
    }
}