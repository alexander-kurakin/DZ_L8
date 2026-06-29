using System.Collections.Generic;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Gameplay.Features.PlantCombatFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.JuiceFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Mines
{
    public class MineSectorDetonationSystem : IInitializableSystem, IUpdatableSystem
    {
        private readonly SectorEnemyQueryService _sectorEnemyQueryService;
        private readonly PlantDamageApplicationService _plantDamageApplicationService;
        private readonly GameplayJuiceService _gameplayJuiceService;

        private readonly List<Entity> _enemiesInSector = new();
        private readonly HashSet<Entity> _damagedEnemies = new();

        private Entity _mineEntity;
        private Transform _mineTransform;
        private ReactiveVariable<float> _mineDamage;
        private ReactiveEvent<Vector3> _dealAreaImpactDamageRequest;
        private PlantSector _plantSector;

        private float _detonationDelayRemaining;
        private bool _isDetonating;
        private float _procDelaySeconds;

        public MineSectorDetonationSystem(
            SectorEnemyQueryService sectorEnemyQueryService,
            PlantDamageApplicationService plantDamageApplicationService,
            GameplayJuiceService gameplayJuiceService,
            float procDelaySeconds)
        {
            _sectorEnemyQueryService = sectorEnemyQueryService;
            _plantDamageApplicationService = plantDamageApplicationService;
            _gameplayJuiceService = gameplayJuiceService;
            _procDelaySeconds = procDelaySeconds;
        }

        public void OnInit(Entity entity)
        {
            _mineEntity = entity;
            _dealAreaImpactDamageRequest = entity.DealAreaImpactDamageRequest;
            _mineTransform = entity.Transform;
            _mineDamage = entity.AreaImpactDamage;
            _plantSector = entity.PlantSectorC;
        }

        public void OnUpdate(float deltaTime)
        {
            if (_isDetonating)
            {
                UpdateDetonation(deltaTime);
                return;
            }

            if (HasUndamagedEnemyInSector())
            {
                _isDetonating = true;
                _detonationDelayRemaining = _procDelaySeconds;
            }
        }

        private void UpdateDetonation(float deltaTime)
        {
            _detonationDelayRemaining -= deltaTime;

            if (_detonationDelayRemaining > 0f)
                return;

            ApplySectorDamageToUndamagedEnemies();
            _gameplayJuiceService.PlayScreenShakeMedium();
            _dealAreaImpactDamageRequest?.Invoke(_mineTransform.position);
            _isDetonating = false;
        }

        private bool HasUndamagedEnemyInSector()
        {
            _sectorEnemyQueryService.CollectEnemiesInSector(_plantSector.Value, _enemiesInSector);

            for (int index = 0; index < _enemiesInSector.Count; index++)
            {
                if (_damagedEnemies.Contains(_enemiesInSector[index]) == false)
                    return true;
            }

            return false;
        }

        private void ApplySectorDamageToUndamagedEnemies()
        {
            _sectorEnemyQueryService.CollectEnemiesInSector(_plantSector.Value, _enemiesInSector);

            float baseDamage = _mineDamage.Value;

            for (int index = 0; index < _enemiesInSector.Count; index++)
            {
                Entity enemy = _enemiesInSector[index];

                if (_damagedEnemies.Contains(enemy))
                    continue;

                bool damageApplied = _plantDamageApplicationService.TryApplyDamage(
                    _mineEntity,
                    enemy,
                    baseDamage,
                    PlantDamageSource.Mine);

                if (damageApplied)
                    _damagedEnemies.Add(enemy);
            }
        }
    }
}
