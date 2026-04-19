using Assets._Project.Develop.Runtime.Configs.Gameplay.Stages;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.Enemies;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System;
using System.Collections.Generic;
using _Project.Develop.Runtime.Gameplay.Features.Input;
using Assets._Project.Develop.Runtime.Gameplay.Features.Ability;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using Assets._Project.Develop.Runtime.Configs.Gameplay.MouseConfig;
using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.MainHero;
using Assets._Project.Develop.Runtime.Utilities;
using Assets._Project.Develop.Runtime.Utilities.ConfigsManagment;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature
{
    public class ClearAllEnemiesStage : IStage
    {
        private readonly ClearAllEnemiesStageConfig _config;
        private readonly RaycastConfig _mouseRaycastConfig;
        private readonly TowerConfig _towerConfig;
        
        private EntitiesLifeContext _entitiesLifeContext;
        private MainHeroHolderService _mainHeroHolderService;
        private EnemiesFactory _enemiesFactory;
        
        private IMouseInputService _mouseInputService;
        private MouseRaycastService _mouseRaycastService;
        
        private ReactiveEvent _completed = new();
        private Entity _mainHero;
        private bool _inProcess;

        private Dictionary<Entity, IDisposable> _spawnedEnemiesToRemoveReason = new();

        public ClearAllEnemiesStage(
            ClearAllEnemiesStageConfig config,
            EnemiesFactory enemiesFactory,
            EntitiesLifeContext entitiesLifeContext,
            ConfigsProviderService  configsProviderService,
            MainHeroHolderService mainHeroHolderService,
            IMouseInputService mouseInputService,
            MouseRaycastService mouseRaycastService)
        {
            _config = config;
            _enemiesFactory = enemiesFactory;
            _entitiesLifeContext = entitiesLifeContext;
            _mainHeroHolderService = mainHeroHolderService;
            _mouseInputService = mouseInputService;
            _mouseRaycastService = mouseRaycastService;
            
            _towerConfig = configsProviderService.GetConfig<TowerConfig>();
            _mouseRaycastConfig = configsProviderService.GetConfig<RaycastConfig>();
            
        }

        public IReadOnlyEvent Completed => _completed;

        public void Cleanup()
        {
            foreach (KeyValuePair<Entity, IDisposable> item in _spawnedEnemiesToRemoveReason)
            {
                item.Value.Dispose();
                _entitiesLifeContext.Release(item.Key);
            }

            _spawnedEnemiesToRemoveReason.Clear();

            _inProcess = false;
        }

        public void Dispose()
        {
            foreach (KeyValuePair<Entity, IDisposable> item in _spawnedEnemiesToRemoveReason)
            {
                item.Value.Dispose();
            }

            _spawnedEnemiesToRemoveReason.Clear();

            _inProcess = false;
        }

        public void Start()
        {
            if (_inProcess)
                throw new InvalidOperationException("Game mode alread started");

            SpawnEnemies();
            
            _mainHero = _mainHeroHolderService.MainHero;
            _mainHero.AbilityUserActiveAbility.Value = AbilityType.ExplodeAtPoint;

            _inProcess = true;
        }

        public void Update(float deltaTime)
        {
            if (_inProcess == false)
                return;

            if (_spawnedEnemiesToRemoveReason.Count == 0)
            {
                ProcessEnd();
                return;
            }

            if (MouseClickedOnGenericLayer(out Vector3 hitPoint))
                    _mainHero.AbilityUserAllAbilities[_mainHero.AbilityUserActiveAbility.Value]
                        .AbilityUseRequest.Invoke(hitPoint);
        }

        private bool MouseClickedOnGenericLayer(out Vector3 hitPoint)
        {
            if (_mouseInputService.FireButtonPressed)
                if (_mouseRaycastService.TryGetHit(
                        _mouseInputService.PointerScreenPosition,
                        out RaycastHit hit,
                        _mouseRaycastConfig.MouseRaycastDistance,
                        Layers.GenericLayerMask))
                {
                    hitPoint = hit.point;
                    return true;
                }

            hitPoint = Vector3.zero;
            return false;
        }

        private void ProcessEnd()
        {
            _inProcess = false;
            _completed.Invoke();
        }

        private void SpawnEnemies()
        {
            foreach (EnemyItemConfig enemyItemConfig in _config.EnemyItems)
                SpawnEnemy(enemyItemConfig);
        }

        private Vector3 GenerateRandomPositionInCircle()
        {
            Vector3 center = _towerConfig.StartPosition;

            Vector2 disk = Random.insideUnitCircle.normalized * _config.EnemySpawnRadius;
            
            return new Vector3(center.x + disk.x, center.y, center.z + disk.y);
        }

        private void SpawnEnemy(EnemyItemConfig enemyItemConfig)
        {
            for (int i = 0; i < enemyItemConfig.EnemiesCount; i++)
            {
                Entity spawnedEnemy =
                    _enemiesFactory.Create(GenerateRandomPositionInCircle(), enemyItemConfig.EnemyConfig);

                IDisposable removeReason = spawnedEnemy.IsDead.Subscribe((oldValue, isDead) =>
                {
                    if (isDead)
                    {
                        IDisposable disposable = _spawnedEnemiesToRemoveReason[spawnedEnemy];
                        disposable.Dispose();
                        _spawnedEnemiesToRemoveReason.Remove(spawnedEnemy);
                    }
                });

                _spawnedEnemiesToRemoveReason.Add(spawnedEnemy, removeReason);
            }
        }
    }
}
