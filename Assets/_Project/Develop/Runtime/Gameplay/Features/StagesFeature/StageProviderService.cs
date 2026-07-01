using Assets._Project.Develop.Runtime.Configs.Gameplay.Levels;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Stages;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature
{
    public class StageProviderService : IDisposable
    {
        private static readonly string[] STAGE_CONFIG_RESOURCE_PATHS =
        {
            "Configs/Gameplay/Stages/Intro1",
            "Configs/Gameplay/Stages/Intro2",
            "Configs/Gameplay/Stages/Intro3",
            "Configs/Gameplay/Stages/Intro4",
            "Configs/Gameplay/Stages/Intro5",
        };

        private static readonly string SURVIVAL_WAVE_RESOURCE_PATH = "Configs/Gameplay/Stages/Intro5";

        private readonly ReactiveEvent _stageCompleted = new();
        private ReactiveVariable<int> _currentStageNumber = new();
        private ReactiveVariable<StageResults> _currentStageResult = new();

        private LevelConfig _levelConfig;
        private StagesFactory _stagesFactory;
        private SurvivalWaveScalingService _survivalWaveScalingService;

        private IStage _currentStage;
        private bool _survivalModeActive;
        private ClearAllEnemiesStageConfig _survivalWaveTemplate;
        
        private List<Entity> _spawnedTemporaryEntities = new();
        private EntitiesLifeContext _entitiesLifeContext;

        private IDisposable _stageEndedDisposable;

        public StageProviderService(
            LevelConfig levelConfig, 
            StagesFactory stagesFactory,
            SurvivalWaveScalingService survivalWaveScalingService,
            EntitiesLifeContext entitiesLifeContext)
        {
            _levelConfig = levelConfig;
            _stagesFactory = stagesFactory;
            _survivalWaveScalingService = survivalWaveScalingService;
            _entitiesLifeContext = entitiesLifeContext;
            ValidateStageConfigs();
        }

        public IReadOnlyVariable<int> CurrentStageNumber => _currentStageNumber;
        public IReadOnlyVariable<StageResults> CurrentStageResult => _currentStageResult;
        public IReadOnlyEvent StageCompleted => _stageCompleted;
        
        public int CurrentStageEnemiesCount
        {
            get
            {
                ClearAllEnemiesWaveRuntimeData waveData =
                    GetWaveRuntimeDataForWave(_currentStageNumber.Value);

                if (waveData == null)
                    return 0;

                return waveData.TotalEnemiesCount;
            }
        }

        public int StagesCount => _levelConfig.StageConfigs.Count;

        public bool IsSurvivalModeActive => _survivalModeActive;

        public void ActivateSurvivalMode()
        {
            if (_survivalModeActive)
                return;

            _survivalModeActive = true;
            _survivalWaveTemplate = Resources.Load<ClearAllEnemiesStageConfig>(SURVIVAL_WAVE_RESOURCE_PATH);

            if (_survivalWaveTemplate == null)
            {
                throw new InvalidOperationException(
                    $"Survival wave config is missing at '{SURVIVAL_WAVE_RESOURCE_PATH}'.");
            }
        }

        public IReadOnlyList<WaveEnemyPreviewType> GetWaveEnemyPreviewTypesForWave(int waveNumber)
        {
            return GetWaveEnemyPreviewTypesAtIndex(waveNumber - 1);
        }

        public bool HasNextStage() => CurrentStageNumber.Value < StagesCount || _survivalModeActive;

        public void ApplyDebugSimulatedCompletedWaves(int completedWaves)
        {
            _currentStageNumber.Value = completedWaves;
            _currentStageResult.Value = StageResults.Completed;
        }

        public ClearAllEnemiesWaveRuntimeData GetWaveRuntimeDataForWave(int waveNumber)
        {
            if (waveNumber > StagesCount && _survivalModeActive)
            {
                int survivalTier = _survivalWaveScalingService.GetSurvivalTierForWave(waveNumber, StagesCount);
                return _survivalWaveScalingService.CreateScaledWaveData(_survivalWaveTemplate, survivalTier);
            }

            int stageIndex = waveNumber - 1;

            if (stageIndex < 0 || stageIndex >= _levelConfig.StageConfigs.Count)
                return null;

            if (ResolveStageConfig(stageIndex) is ClearAllEnemiesStageConfig clearAllEnemiesStageConfig == false)
                return null;

            return ClearAllEnemiesWaveRuntimeData.FromConfig(clearAllEnemiesStageConfig);
        }

        public ClearAllEnemiesStageConfig GetClearAllEnemiesStageConfigForWave(int waveNumber)
        {
            ClearAllEnemiesWaveRuntimeData waveData = GetWaveRuntimeDataForWave(waveNumber);

            if (waveData == null)
                return null;

            if (waveNumber > StagesCount && _survivalModeActive)
                return _survivalWaveTemplate;

            int stageIndex = waveNumber - 1;
            return ResolveStageConfig(stageIndex) as ClearAllEnemiesStageConfig;
        }

        public void SwitchToNext()
        {
            if (HasNextStage() == false)
                throw new InvalidOperationException();

            if (_currentStage != null)
                CleanupCurrent();

            _currentStageNumber.Value++;
            _currentStageResult.Value = StageResults.Uncompleted;

            ClearAllEnemiesWaveRuntimeData waveData = GetWaveRuntimeDataForWave(_currentStageNumber.Value);

            if (waveData == null)
                throw new InvalidOperationException($"Wave runtime data is missing for wave {_currentStageNumber.Value}.");

            _currentStage = _stagesFactory.Create(waveData);
        }
        
        public void AddTemporaryEntity(Entity entity)
        {
            if (entity != null && !_spawnedTemporaryEntities.Contains(entity))
                _spawnedTemporaryEntities.Add(entity);
        }

        private void ClearTemporaryEntities()
        {
            foreach (Entity entity in _spawnedTemporaryEntities)
                _entitiesLifeContext.Release(entity);
            
            _spawnedTemporaryEntities.Clear();
        }

        public void StartCurrent()
        {
            _stageEndedDisposable = _currentStage.Completed.Subscribe(OnStageCompleted);
            _currentStage.Start();
        }

        private void OnStageCompleted()
        {
            _currentStageResult.Value = StageResults.Completed;
            ClearTemporaryEntities();
            _stageCompleted.Invoke();
        }

        public void UpdateCurrent(float deltaTime) => _currentStage.Update(deltaTime);

        public void CleanupCurrent() => _currentStage.Cleanup();

        public void Dispose()
        {
            _currentStage?.Dispose();
            _stageEndedDisposable?.Dispose();
        }

        private IReadOnlyList<WaveEnemyPreviewType> GetWaveEnemyPreviewTypesAtIndex(int stageConfigIndex)
        {
            if (stageConfigIndex < 0 || stageConfigIndex >= _levelConfig.StageConfigs.Count)
                return Array.Empty<WaveEnemyPreviewType>();

            if (_levelConfig.StageConfigs[stageConfigIndex] is ClearAllEnemiesStageConfig clearAllEnemiesStageConfig == false)
                return Array.Empty<WaveEnemyPreviewType>();

            List<WaveEnemyPreviewType> previewTypes = new();
            HashSet<WaveEnemyPreviewType> seenPreviewTypes = new();

            foreach (SpawnGroupConfig spawnGroup in clearAllEnemiesStageConfig.SpawnGroups)
            {
                foreach (EnemyItemConfig enemyItem in spawnGroup.EnemyItems)
                {
                    WaveEnemyPreviewType previewType = WaveEnemyPreviewResolver.Resolve(enemyItem.EnemyConfig);

                    if (seenPreviewTypes.Add(previewType))
                        previewTypes.Add(previewType);
                }
            }

            return previewTypes;
        }

        private void ValidateStageConfigs()
        {
            for (int stageIndex = 0; stageIndex < _levelConfig.StageConfigs.Count; stageIndex++)
            {
                if (_levelConfig.StageConfigs[stageIndex] != null)
                    continue;

                if (stageIndex >= STAGE_CONFIG_RESOURCE_PATHS.Length)
                {
                    throw new InvalidOperationException(
                        $"Stage config for wave {stageIndex + 1} is missing in LevelConfig.");
                }

                StageConfig fallbackStageConfig = Resources.Load<StageConfig>(STAGE_CONFIG_RESOURCE_PATHS[stageIndex]);

                if (fallbackStageConfig == null)
                {
                    throw new InvalidOperationException(
                        $"Stage config for wave {stageIndex + 1} is missing at '{STAGE_CONFIG_RESOURCE_PATHS[stageIndex]}'.");
                }
            }
        }

        private StageConfig ResolveStageConfig(int stageIndex)
        {
            if (stageIndex >= _levelConfig.StageConfigs.Count)
            {
                if (_survivalModeActive && _survivalWaveTemplate != null)
                    return _survivalWaveTemplate;

                throw new InvalidOperationException(
                    $"Wave index {stageIndex} is out of range for level with {_levelConfig.StageConfigs.Count} stages.");
            }

            if (stageIndex < 0)
            {
                throw new InvalidOperationException(
                    $"Wave index {stageIndex} is out of range for level with {_levelConfig.StageConfigs.Count} stages.");
            }

            StageConfig stageConfig = _levelConfig.StageConfigs[stageIndex];

            if (stageConfig != null)
                return stageConfig;

            if (stageIndex >= STAGE_CONFIG_RESOURCE_PATHS.Length)
            {
                throw new InvalidOperationException(
                    $"Stage config for wave {stageIndex + 1} is missing in LevelConfig.");
            }

            StageConfig fallbackStageConfig = Resources.Load<StageConfig>(STAGE_CONFIG_RESOURCE_PATHS[stageIndex]);

            if (fallbackStageConfig == null)
            {
                throw new InvalidOperationException(
                    $"Stage config for wave {stageIndex + 1} is missing at '{STAGE_CONFIG_RESOURCE_PATHS[stageIndex]}'.");
            }

            return fallbackStageConfig;
        }
    }
}
