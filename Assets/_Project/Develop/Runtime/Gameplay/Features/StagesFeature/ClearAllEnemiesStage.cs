using Assets._Project.Develop.Runtime.Configs.Gameplay.Stages;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.Enemies;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System;
using System.Collections.Generic;
using _Project.Develop.Runtime.Gameplay.Features.Input;
using _Project.Develop.Runtime.Meta.Features.Powerups;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using Assets._Project.Develop.Runtime.Configs.Gameplay.MouseConfig;
using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.MainHero;
using Assets._Project.Develop.Runtime.Utilities;
using Assets._Project.Develop.Runtime.Utilities.Audio;
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
        private IBackgroundMusicService _backgroundMusicService;
        private PermanentPowerupResolver _permanentPowerupResolver;
        
        private ReactiveEvent _completed = new();
        private Entity _mainHero;
        private Entity _towerWalker;
        private bool _inProcess;

        private int _targetDebuffedEnemiesCount;
        private int _currentDebuffedEnemiesCount;
        private float _debuffPercent;
        private bool _shouldDebuff;

        private Dictionary<Entity, IDisposable> _spawnedEnemiesToRemoveReason = new();

        public ClearAllEnemiesStage(
            ClearAllEnemiesStageConfig config,
            EnemiesFactory enemiesFactory,
            EntitiesLifeContext entitiesLifeContext,
            ConfigsProviderService  configsProviderService,
            MainHeroHolderService mainHeroHolderService,
            IMouseInputService mouseInputService,
            MouseRaycastService mouseRaycastService,
            IBackgroundMusicService  backgroundMusicService,
            PermanentPowerupResolver permanentPowerupResolver)
        {
            _config = config;
            _enemiesFactory = enemiesFactory;
            _entitiesLifeContext = entitiesLifeContext;
            _mainHeroHolderService = mainHeroHolderService;
            _mouseInputService = mouseInputService;
            _mouseRaycastService = mouseRaycastService;
            _backgroundMusicService = backgroundMusicService;
            _permanentPowerupResolver = permanentPowerupResolver;
            
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
                throw new InvalidOperationException("Game mode already started");
            
            _backgroundMusicService.Play(BackgroundMusicTrackIDs.Battle);
            
            _mainHero = _mainHeroHolderService.MainHero;
            _towerWalker = _mainHeroHolderService.TowerWalker;

            TryApplyStagePowerups();
            
            SpawnEnemies();
            
            _inProcess = true;
        }

        private void TryApplyStagePowerups()
        {
            float maxHealth = _mainHero.MaxHealth.Value;
            float currentHealth = _mainHero.CurrentHealth.Value;

            if (_permanentPowerupResolver.TryGetTowerHealMult(out float modifierMult))
            {
                float newCurrentHealth = MathF.Min(maxHealth, currentHealth + maxHealth * modifierMult);
                _mainHero.CurrentHealth.Value = newCurrentHealth;
            }

            if (_permanentPowerupResolver.TryGetFirstEnemiesDamageMult(out float debuffPerc,
                    out int debuffEnemiesCount))
            {
                _targetDebuffedEnemiesCount = debuffEnemiesCount;
                _debuffPercent = debuffPerc;
                _currentDebuffedEnemiesCount = 0;

                _shouldDebuff = true;
            }
            else
            {
                _shouldDebuff = false;
            }
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
            {
                if (_towerWalker != null)
                    _towerWalker.MagicCastRequestedEvent.Invoke(hitPoint);
                
                _mainHero.AbilityUserAllAbilities[_mainHero.AbilityUserActiveAbility.Value]
                    .AbilityUseRequest.Invoke(hitPoint);
            }
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
                if (_shouldDebuff && _currentDebuffedEnemiesCount >= _targetDebuffedEnemiesCount)
                    _shouldDebuff = false;
                
                float healthDebuffPercent = 0f;
                
                if (_shouldDebuff)
                    healthDebuffPercent = _debuffPercent;

                Entity spawnedEnemy = _enemiesFactory.Create(GenerateRandomPositionInCircle(), enemyItemConfig.EnemyConfig, healthDebuffPercent);

                if (_shouldDebuff)
                    _currentDebuffedEnemiesCount++;

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
