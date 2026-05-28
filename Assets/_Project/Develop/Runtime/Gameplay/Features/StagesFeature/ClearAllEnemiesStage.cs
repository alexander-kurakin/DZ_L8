using Assets._Project.Develop.Runtime.Configs.Gameplay.Stages;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.Enemies;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System;
using System.Collections.Generic;
using _Project.Develop.Runtime.Gameplay.Features.Input;
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
        private const int SpawnMaxSectors = 12;
        
        private readonly ClearAllEnemiesStageConfig _config;
        private readonly RaycastConfig _mouseRaycastConfig;
        private readonly TowerConfig _towerConfig;
        
        private EntitiesLifeContext _entitiesLifeContext;
        private MainHeroHolderService _mainHeroHolderService;
        private EnemiesFactory _enemiesFactory;
        
        private IMouseInputService _mouseInputService;
        private MouseRaycastService _mouseRaycastService;
        private IBackgroundMusicService _backgroundMusicService;
        
        private ReactiveEvent _completed = new();
        
        private Entity _mainHero;
        private Entity _towerWalker;
        private bool _inProcess;

        private Dictionary<Entity, IDisposable> _spawnedEnemiesToRemoveReason = new();
        private readonly int[] _spawnSectorOrder = new int[SpawnMaxSectors];
        private int _spawnSectorCursor;


        private readonly Queue<SpawnGroupConfig> _spawnGroupsQueue = new();
        private readonly Queue<EntityConfig> _currentEnemyQueue = new();
        private SpawnGroupConfig _currentSpawnGroup;
        private float _secondsUntilNextEnemySpawn;
        private bool _waitingGroupPause;
        
        
        public ClearAllEnemiesStage(
            ClearAllEnemiesStageConfig config,
            EnemiesFactory enemiesFactory,
            EntitiesLifeContext entitiesLifeContext,
            ConfigsProviderService  configsProviderService,
            MainHeroHolderService mainHeroHolderService,
            IMouseInputService mouseInputService,
            MouseRaycastService mouseRaycastService,
            IBackgroundMusicService  backgroundMusicService)
        {
            _config = config;
            _enemiesFactory = enemiesFactory;
            _entitiesLifeContext = entitiesLifeContext;
            _mainHeroHolderService = mainHeroHolderService;
            _mouseInputService = mouseInputService;
            _mouseRaycastService = mouseRaycastService;
            _backgroundMusicService = backgroundMusicService;
            
            _towerConfig = configsProviderService.GetConfig<TowerConfig>();
            _mouseRaycastConfig = configsProviderService.GetConfig<RaycastConfig>();
            
        }

        public IReadOnlyEvent Completed => _completed;
        
        public void Start()
        {
            if (_inProcess)
                throw new InvalidOperationException("Game mode already started");
            
            _backgroundMusicService.Play(BackgroundMusicTrackIDs.Battle);
            
            _mainHero = _mainHeroHolderService.MainHero;
            _towerWalker = _mainHeroHolderService.TowerWalker;

            ResetSpawnSectorOrderForWave();
            PrepareGroupsQueue();
            MoveToNextGroupOrFinishSpawn();
            
            _mouseInputService.HideCursor();
            _inProcess = true;
        }
        
        public void Update(float deltaTime)
        {
            if (_inProcess == false)
                return;

            ProcessSpawn(deltaTime);

            if (IsSpawnCompleted() && _spawnedEnemiesToRemoveReason.Count == 0)
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

         private void ProcessSpawn(float deltaTime)
        {
            if (IsSpawnCompleted())
                return;
            
            _secondsUntilNextEnemySpawn -= deltaTime;
            if (_secondsUntilNextEnemySpawn > 0f)
                return;
            
            if (_waitingGroupPause)
            {
                _waitingGroupPause = false;
                MoveToNextGroupOrFinishSpawn();
                return;
            }

            if (_currentEnemyQueue.Count == 0)
            {
                _waitingGroupPause = true;
                _secondsUntilNextEnemySpawn = _currentSpawnGroup.PauseAfterGroup;
                return;
            }
            
            EntityConfig enemyConfig = _currentEnemyQueue.Dequeue();
            SpawnSingleEnemyAtNextSectorPosition(enemyConfig);
            
            if (_currentEnemyQueue.Count > 0)
            {
                _secondsUntilNextEnemySpawn = Random.Range(
                    _currentSpawnGroup.MinTimeBetweenSpawns,
                    _currentSpawnGroup.MaxTimeBetweenSpawns);
            }
            else
            {
                _waitingGroupPause = true;
                _secondsUntilNextEnemySpawn = _currentSpawnGroup.PauseAfterGroup;
            }
        }
         
        private void PrepareGroupsQueue()
        {
            _spawnGroupsQueue.Clear();
            _currentEnemyQueue.Clear();
            
            foreach (SpawnGroupConfig spawnConfigGroup in _config.SpawnGroups)
                _spawnGroupsQueue.Enqueue(spawnConfigGroup);
        }
        
        private void MoveToNextGroupOrFinishSpawn()
        {
            _currentEnemyQueue.Clear();
            
            if (_spawnGroupsQueue.Count == 0)
            {
                _currentSpawnGroup = null;
                return;
            }
            
            _currentSpawnGroup = _spawnGroupsQueue.Dequeue();
            
            foreach (EnemyItemConfig enemyItem in _currentSpawnGroup.EnemyItems)
            {
                for (int i = 0; i < enemyItem.EnemiesCount; i++)
                    _currentEnemyQueue.Enqueue(enemyItem.EnemyConfig);
            }
            
            _secondsUntilNextEnemySpawn = 0f;
        }
        
        private bool IsSpawnCompleted() =>
            _spawnGroupsQueue.Count == 0 &&
            _currentEnemyQueue.Count == 0 &&
            _currentSpawnGroup == null &&
            _waitingGroupPause == false;

        public void Cleanup()
        {
            foreach (KeyValuePair<Entity, IDisposable> item in _spawnedEnemiesToRemoveReason)
            {
                item.Value.Dispose();
                _entitiesLifeContext.Release(item.Key);
            }

            _spawnedEnemiesToRemoveReason.Clear();
            _spawnGroupsQueue.Clear();
            _currentEnemyQueue.Clear();

            _currentSpawnGroup = null;
            _waitingGroupPause = false;
            _inProcess = false;
        }

        public void Dispose()
        {
            foreach (KeyValuePair<Entity, IDisposable> item in _spawnedEnemiesToRemoveReason)
                item.Value.Dispose();

            _spawnedEnemiesToRemoveReason.Clear();
            _spawnGroupsQueue.Clear();
            _currentEnemyQueue.Clear();

            _currentSpawnGroup = null;
            _waitingGroupPause = false;
            _inProcess = false;
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
            _mouseInputService.ShowCursor();
            _completed.Invoke();
        }

        private Vector3 GenerateRandomPositionInCircleShuffledSectors()
        {
            Vector3 center = _towerConfig.StartPosition;
            float ringRadius = _config.EnemySpawnRadius * Random.Range(0.9f, 1f);

            float sectorWidthRadians = (Mathf.PI * 2f) / SpawnMaxSectors;
            int sectorIndex = _spawnSectorOrder[_spawnSectorCursor % SpawnMaxSectors];
            
            _spawnSectorCursor++;

            float angleRadians =
                sectorIndex * sectorWidthRadians +
                Random.Range(0f, sectorWidthRadians);

            float offsetX = Mathf.Cos(angleRadians) * ringRadius;
            float offsetZ = Mathf.Sin(angleRadians) * ringRadius;

            return new Vector3(center.x + offsetX, center.y, center.z + offsetZ);
        }
        
        private void ResetSpawnSectorOrderForWave()
        {
            for (int index = 0; index < SpawnMaxSectors; index++)
                _spawnSectorOrder[index] = index;

            for (int lastIndex = SpawnMaxSectors - 1; lastIndex > 0; lastIndex--)
            {
                int randomIndex = Random.Range(0, lastIndex + 1);
                (_spawnSectorOrder[lastIndex], _spawnSectorOrder[randomIndex]) = (_spawnSectorOrder[randomIndex], _spawnSectorOrder[lastIndex]);
            }

            _spawnSectorCursor = 0;
        }
        
        private void SpawnSingleEnemyAtNextSectorPosition(EntityConfig enemyConfig)
        {
            Vector3 spawnPosition = GenerateRandomPositionInCircleShuffledSectors();
            Entity spawnedEnemy = _enemiesFactory.Create(spawnPosition, enemyConfig);

            IDisposable removeReason = spawnedEnemy.InDeathProcess.Subscribe((oldValue, inDead) =>
            {
                if (inDead == false && spawnedEnemy.IsDead.Value)
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
