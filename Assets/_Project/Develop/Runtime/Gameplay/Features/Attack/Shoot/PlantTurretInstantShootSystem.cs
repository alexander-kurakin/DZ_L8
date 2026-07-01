using Assets._Project.Develop.Runtime.Configs.Gameplay.Stages;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Gameplay.Features.JuiceFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.PlantBuildingBuff;
using Assets._Project.Develop.Runtime.Gameplay.Features.PlantCombatFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.TakeDamage;
using Assets._Project.Develop.Runtime.Gameplay.Features.TeamsFeature;
using Assets._Project.Develop.Runtime.Utilities.Conditions;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Attack.Shoot
{
    public class PlantTurretInstantShootSystem : IInitializableSystem, IDisposableSystem
    {
        private readonly EntitiesFactory _entitiesFactory;
        private readonly SectorEnemyQueryService _sectorEnemyQueryService;
        private readonly PlantDamageApplicationService _plantDamageApplicationService;
        private readonly PlantDamageCounterService _plantDamageCounterService;
        private readonly GameplayJuiceService _gameplayJuiceService;
        private readonly PlantBuildingBuffService _plantBuildingBuffService;
        private readonly PlantBuildingBuffJuiceService _plantBuildingBuffJuiceService;

        private readonly List<Entity> _enemiesInSector = new();

        private ReactiveEvent _attackDelayEndEvent;
        private Entity _entity;
        private ReactiveVariable<float> _damage;
        private Transform _shootPoint;
        private TurretPathTargetSelector _targetSelector;
        private IDisposable _attackDelayEndDisposable;

        public PlantTurretInstantShootSystem(
            EntitiesFactory entitiesFactory,
            SectorEnemyQueryService sectorEnemyQueryService,
            PlantDamageApplicationService plantDamageApplicationService,
            PlantDamageCounterService plantDamageCounterService,
            GameplayJuiceService gameplayJuiceService,
            PlantBuildingBuffService plantBuildingBuffService,
            PlantBuildingBuffJuiceService plantBuildingBuffJuiceService)
        {
            _entitiesFactory = entitiesFactory;
            _sectorEnemyQueryService = sectorEnemyQueryService;
            _plantDamageApplicationService = plantDamageApplicationService;
            _plantDamageCounterService = plantDamageCounterService;
            _gameplayJuiceService = gameplayJuiceService;
            _plantBuildingBuffService = plantBuildingBuffService;
            _plantBuildingBuffJuiceService = plantBuildingBuffJuiceService;
        }

        public void OnInit(Entity entity)
        {
            _entity = entity;
            _attackDelayEndEvent = entity.AttackDelayEndEvent;
            _damage = entity.InstantAttackDamage;
            _shootPoint = entity.ShootPoint;
            _targetSelector = new TurretPathTargetSelector(entity, _sectorEnemyQueryService);
            _attackDelayEndDisposable = _attackDelayEndEvent.Subscribe(OnAttackDelayEnd);
        }

        private void OnAttackDelayEnd()
        {
            if (_entity.TryGetPlantSector(out SectorId plantSector) == false)
            {
                FireProjectile(null);
                return;
            }

            Entity priorityTarget = _targetSelector.SelectTargetFrom(null);

            if (priorityTarget != null
                && IsValidDamageTarget(priorityTarget)
                && TurretTargetPriority.IsInExactSector(priorityTarget, plantSector) == false
                && TryApplyDirectDamageToTarget(priorityTarget))
            {
                _gameplayJuiceService.PlayTurretShot();
                return;
            }

            _sectorEnemyQueryService.CollectEnemiesInSector(plantSector, _enemiesInSector);

            if (HasAnyValidDamageTarget(_enemiesInSector)
                && TryApplyDirectDamageOnCell(plantSector))
            {
                _gameplayJuiceService.PlayTurretShot();
                return;
            }

            FireProjectile(priorityTarget);
        }

        private bool TryApplyDirectDamageToTarget(Entity target)
        {
            bool damageApplied = _plantDamageApplicationService.TryApplyDamage(
                _entity,
                target,
                _damage.Value,
                PlantDamageSource.Turret);

            if (damageApplied == false)
                return false;

            _gameplayJuiceService.PlayTurretHit(target);

            if (_plantBuildingBuffJuiceService.IsBuffed(_entity))
                _plantBuildingBuffJuiceService.PlayBuffedTurretHit();

            return true;
        }

        private bool TryApplyDirectDamageOnCell(SectorId plantSector)
        {
            _sectorEnemyQueryService.CollectEnemiesInSector(plantSector, _enemiesInSector);

            bool hasDragonOnCell = false;

            for (int index = 0; index < _enemiesInSector.Count; index++)
            {
                Entity enemy = _enemiesInSector[index];

                if (IsValidDamageTarget(enemy) == false)
                    continue;

                if (TurretTargetPriority.IsDragon(enemy))
                {
                    hasDragonOnCell = true;
                    break;
                }
            }

            float baseDamage = _damage.Value;
            bool anyDamageApplied = false;

            for (int index = 0; index < _enemiesInSector.Count; index++)
            {
                Entity enemy = _enemiesInSector[index];

                if (IsValidDamageTarget(enemy) == false)
                    continue;

                if (hasDragonOnCell && TurretTargetPriority.IsDragon(enemy) == false)
                    continue;

                bool damageApplied = _plantDamageApplicationService.TryApplyDamage(
                    _entity,
                    enemy,
                    baseDamage,
                    PlantDamageSource.Turret);

                if (damageApplied == false)
                    continue;

                anyDamageApplied = true;
                _gameplayJuiceService.PlayTurretHit(enemy);

                if (_plantBuildingBuffJuiceService.IsBuffed(_entity))
                    _plantBuildingBuffJuiceService.PlayBuffedTurretHit();
            }

            return anyDamageApplied;
        }

        private bool HasAnyValidDamageTarget(List<Entity> enemies)
        {
            for (int index = 0; index < enemies.Count; index++)
            {
                if (IsValidDamageTarget(enemies[index]))
                    return true;
            }

            return false;
        }

        private void FireProjectile(Entity aimTarget)
        {
            float shotDamage = ResolveShotDamage(aimTarget ?? _entity.CurrentTarget.Value);
            Vector3 direction = ResolveShotDirection(aimTarget ?? _entity.CurrentTarget.Value);

            _entitiesFactory.CreateProjectile(
                _shootPoint.position,
                direction,
                shotDamage,
                25f,
                _entity,
                TakeDamageVisualKind.Turret);

            _gameplayJuiceService.PlayTurretShot();
        }

        private Vector3 ResolveShotDirection(Entity target)
        {
            if (target != null && target.TryGetTransform(out Transform targetTransform))
            {
                Vector3 direction = targetTransform.position - _shootPoint.position;
                direction.y = 0f;

                if (direction.sqrMagnitude > 0.0001f)
                    return direction.normalized;
            }

            return _shootPoint.forward;
        }

        private bool IsValidDamageTarget(Entity target)
        {
            if (target.HasComponent<TakeDamageRequest>() == false)
                return false;

            if (target.TryGetCanTakeDamage(out ICompositeCondition canApplyDamage))
            {
                if (canApplyDamage.Evaluate() == false)
                    return false;
            }

            if (_entity.TryGetTeam(out ReactiveVariable<Teams> sourceTeam)
                && target.TryGetTeam(out ReactiveVariable<Teams> targetTeam))
            {
                if (sourceTeam.Value == targetTeam.Value)
                    return false;
            }

            if (target == _entity)
                return false;

            return true;
        }

        private float ResolveShotDamage(Entity target)
        {
            float buildingBuffMultiplier = _plantBuildingBuffService.GetDamageMultiplier(_entity);

            if (target == null)
                return _damage.Value * buildingBuffMultiplier;

            if (target.TryGetEnemyWavePreviewType(out WaveEnemyPreviewType previewType) == false)
                return _damage.Value * buildingBuffMultiplier;

            float damageMultiplier = _plantDamageCounterService.GetDamageMultiplier(PlantDamageSource.Turret, previewType);
            return _damage.Value * damageMultiplier * buildingBuffMultiplier;
        }

        public void OnDispose()
        {
            _attackDelayEndDisposable.Dispose();
        }
    }
}
