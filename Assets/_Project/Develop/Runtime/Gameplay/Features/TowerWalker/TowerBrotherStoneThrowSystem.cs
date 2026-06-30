using System.Collections.Generic;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Gameplay.Features.AI;
using Assets._Project.Develop.Runtime.Gameplay.Features.GameplayStateBridge;
using Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.TowerWalker
{
    public class TowerBrotherStoneThrowSystem : IInitializableSystem, IUpdatableSystem
    {
        private const float MIN_DIRECTION_SQR_MAGNITUDE = 0.0001f;

        private readonly SectorEnemyQueryService _sectorEnemyQueryService;
        private readonly EntitiesFactory _entitiesFactory;
        private readonly TowerBrotherStoneThrowConfig _config;
        private readonly BrainsFactory _brainsFactory;

        private readonly List<Entity> _targetEnemies = new();

        private Entity _brotherEntity;
        private ReactiveVariable<GameplayStates> _gameplayPhase;
        private ReactiveEvent<Entity> _stoneThrowEvent;
        private ReactiveVariable<bool> _isStoneThrowing;
        private float _cooldownRemaining;

        public TowerBrotherStoneThrowSystem(
            SectorEnemyQueryService sectorEnemyQueryService,
            EntitiesFactory entitiesFactory,
            TowerBrotherStoneThrowConfig config,
            BrainsFactory brainsFactory)
        {
            _sectorEnemyQueryService = sectorEnemyQueryService;
            _entitiesFactory = entitiesFactory;
            _config = config;
            _brainsFactory = brainsFactory;
        }

        public void OnInit(Entity entity)
        {
            _brotherEntity = entity;
            _gameplayPhase = entity.GameplayPhase;
            _stoneThrowEvent = entity.BrotherStoneThrowEvent;
            _isStoneThrowing = entity.BrotherStoneThrowing;
            _cooldownRemaining = 0f;
        }

        public void OnUpdate(float deltaTime)
        {
            if (_gameplayPhase.Value != GameplayStates.StageProcess)
                return;

            if (_config.ThrowIntervalSeconds <= 0f || _config.DamagePerThrow <= 0f)
                return;

            _cooldownRemaining -= deltaTime;

            if (_cooldownRemaining > 0f)
                return;

            if (TryThrowStoneAtTarget() == false)
                return;
        }

        private bool TryThrowStoneAtTarget()
        {
            _sectorEnemyQueryService.CollectEnemiesOnBelts(
                SectorBelt.Middle,
                SectorBelt.Inner,
                _targetEnemies);

            if (_targetEnemies.Count == 0)
                return false;

            if (TryGetBrotherSpawnPosition(out Vector3 spawnPosition) == false)
                return false;

            Entity target = PickLowestHealthEnemy(_targetEnemies, spawnPosition);

            if (target == null)
                return false;

            Vector3 aimPoint = BrotherStoneThrowAimUtility.GetPredictedAimPoint(
                spawnPosition,
                target,
                _config.ProjectileSpeed);

            Vector3 direction = aimPoint - spawnPosition;

            if (direction.sqrMagnitude <= MIN_DIRECTION_SQR_MAGNITUDE)
                return false;

            direction.Normalize();

            _cooldownRemaining = _config.ThrowIntervalSeconds;

            Entity projectile = _entitiesFactory.CreateBrotherStoneProjectile(
                spawnPosition,
                aimPoint,
                target,
                _config.DamagePerThrow,
                _config.ProjectileSpeed,
                _brotherEntity);

            _isStoneThrowing.Value = true;
            _brainsFactory.TryRestartBrotherWalkerIdleCycle(_brotherEntity);
            _stoneThrowEvent.Invoke(projectile);
            return true;
        }

        private static Entity PickLowestHealthEnemy(List<Entity> enemies, Vector3 fromPosition)
        {
            Entity priorityEnemy = null;
            float lowestHealth = float.MaxValue;
            float nearestDistanceSqrAtLowestHealth = float.MaxValue;

            for (int index = 0; index < enemies.Count; index++)
            {
                Entity enemy = enemies[index];

                if (enemy.TryGetCurrentHealth(out ReactiveVariable<float> currentHealth) == false)
                    continue;

                if (currentHealth.Value <= 0f)
                    continue;

                Vector3 aimPoint = BrotherStoneThrowAimUtility.GetEnemyAimPoint(enemy);
                float distanceSqr = (aimPoint - fromPosition).sqrMagnitude;
                float health = currentHealth.Value;

                if (health > lowestHealth)
                    continue;

                if (health < lowestHealth)
                {
                    lowestHealth = health;
                    nearestDistanceSqrAtLowestHealth = distanceSqr;
                    priorityEnemy = enemy;
                    continue;
                }

                if (distanceSqr >= nearestDistanceSqrAtLowestHealth)
                    continue;

                nearestDistanceSqrAtLowestHealth = distanceSqr;
                priorityEnemy = enemy;
            }

            return priorityEnemy;
        }

        private bool TryGetBrotherSpawnPosition(out Vector3 spawnPosition)
        {
            spawnPosition = default;

            if (_brotherEntity.TryGetRigidbody(out Rigidbody brotherRigidbody))
            {
                spawnPosition = brotherRigidbody.position + Vector3.up * _config.ProjectileSpawnHeight;
                return true;
            }

            if (_brotherEntity.TryGetTransform(out Transform brotherTransform))
            {
                spawnPosition = brotherTransform.position + Vector3.up * _config.ProjectileSpawnHeight;
                return true;
            }

            return false;
        }
    }
}
