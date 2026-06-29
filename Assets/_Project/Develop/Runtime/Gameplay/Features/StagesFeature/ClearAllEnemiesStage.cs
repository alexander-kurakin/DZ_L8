using Assets._Project.Develop.Runtime.Configs.Gameplay.Stages;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.Enemies;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System;
using System.Collections.Generic;
using _Project.Develop.Runtime.Gameplay.Features.Input;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using Assets._Project.Develop.Runtime.Gameplay.Features.Ability;
using _Project.Develop.Runtime.Gameplay.Features.ExplosionAbilityPreview;
using Assets._Project.Develop.Runtime.Configs.Gameplay.MouseConfig;
using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.MainHero;
using Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature;
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
        private readonly SectorRegistryService _sectorRegistryService;
        private readonly WaveSpawnPlanService _waveSpawnPlanService;
        
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
        private readonly int[] _spawnPathIndices = new int[SectorId.SectorsPerRing];
        private int _spawnPathCount;
        private int _currentGroupPathIndex = -1;
        private int _currentGroupIndex;


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
            IBackgroundMusicService  backgroundMusicService,
            SectorRegistryService sectorRegistryService,
            WaveSpawnPlanService waveSpawnPlanService)
        {
            _config = config;
            _enemiesFactory = enemiesFactory;
            _entitiesLifeContext = entitiesLifeContext;
            _mainHeroHolderService = mainHeroHolderService;
            _mouseInputService = mouseInputService;
            _mouseRaycastService = mouseRaycastService;
            _backgroundMusicService = backgroundMusicService;
            _sectorRegistryService = sectorRegistryService;
            _waveSpawnPlanService = waveSpawnPlanService;

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

            ResetSpawnPathOrderForWave();
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
            
            if (MouseClickedOnPlacementSurface(out Vector3 hitPoint))
            {
                AbilityType activeAbility = _mainHero.AbilityUserActiveAbility.Value;
                _mainHero.AbilityUserAllAbilities[activeAbility].AbilityUseRequest.Invoke(hitPoint);

                if (activeAbility == AbilityType.ExplodeAtPoint
                    && _towerWalker != null
                    && LmbFrostProjectileService.Instance.HasQueuedProjectileLaunch())
                {
                    _towerWalker.MagicCastRequestedEvent.Invoke(hitPoint);
                }
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
            _currentGroupIndex = 0;

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

            if (_waveSpawnPlanService.TryGetPlannedPathIndexForGroup(_currentGroupIndex, out int plannedPathIndex))
                _currentGroupPathIndex = plannedPathIndex;
            else
                _currentGroupPathIndex = PickRandomUnlockedPathIndex();

            _currentGroupIndex++;

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
            _currentGroupPathIndex = -1;
            _currentGroupIndex = 0;
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
            _currentGroupPathIndex = -1;
            _currentGroupIndex = 0;
            _inProcess = false;
        }
        
        private bool MouseClickedOnPlacementSurface(out Vector3 hitPoint)
        {
            hitPoint = Vector3.zero;

            if (_mouseInputService.FireButtonPressed == false)
                return false;

            return SectorSurfaceClickUtility.TryGetArenaPlanePoint(
                _mouseRaycastService,
                _mouseInputService.PointerScreenPosition,
                _sectorRegistryService,
                out hitPoint);
        }

        private void ProcessEnd()
        {
            _inProcess = false;
            _mouseInputService.ShowCursor();
            _completed.Invoke();
        }

        private Vector3 GenerateSpawnPositionInNextUnlockedPath()
        {
            if (_spawnPathCount == 0)
                throw new InvalidOperationException("No unlocked spawn paths configured.");

            int pathIndex = _currentGroupPathIndex >= 0
                ? _currentGroupPathIndex
                : PickRandomUnlockedPathIndex();

            return _sectorRegistryService.GetRandomSpawnPositionInWedge(pathIndex);
        }

        private void ResetSpawnPathOrderForWave()
        {
            _spawnPathCount = _sectorRegistryService.UnlockedPathCount;
            _currentGroupPathIndex = -1;

            for (int index = 0; index < _spawnPathCount; index++)
                _spawnPathIndices[index] = _sectorRegistryService.GetUnlockedPathIndexAt(index);
        }

        private int PickRandomUnlockedPathIndex()
        {
            if (_spawnPathCount == 0)
                throw new InvalidOperationException("No unlocked spawn paths configured.");

            return _spawnPathIndices[Random.Range(0, _spawnPathCount)];
        }

        private void SpawnSingleEnemyAtNextSectorPosition(EntityConfig enemyConfig)
        {
            Vector3 spawnPosition = GenerateSpawnPositionInNextUnlockedPath();
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
