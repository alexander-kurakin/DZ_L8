using Assets._Project.Develop.Runtime.Configs.Gameplay.Levels;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Stages;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System;
using System.Collections.Generic;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature
{
    public class StageProviderService : IDisposable
    {
        private readonly ReactiveEvent _stageCompleted = new();
        private readonly ReactiveVariable<int> _currentStageNumber = new();
        private readonly ReactiveVariable<StageResults> _currentStageResult = new();

        private readonly LevelConfig _levelConfig;
        private readonly StagesFactory _stagesFactory;
        private readonly EntitiesLifeContext _entitiesLifeContext;

        private IStage _currentStage;
        private readonly List<Entity> _spawnedTemporaryEntities = new();
        private IDisposable _stageEndedDisposable;

        public StageProviderService(
            LevelConfig levelConfig,
            StagesFactory stagesFactory,
            EntitiesLifeContext entitiesLifeContext)
        {
            _levelConfig = levelConfig;
            _stagesFactory = stagesFactory;
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

        public bool HasNextStage() => CurrentStageNumber.Value < StagesCount;

        public void ApplyDebugSimulatedCompletedWaves(int completedWaves)
        {
            _currentStageNumber.Value = completedWaves;
            _currentStageResult.Value = StageResults.Completed;
        }

        public ClearAllEnemiesWaveRuntimeData GetWaveRuntimeDataForWave(int waveNumber)
        {
            int stageIndex = waveNumber - 1;

            if (stageIndex < 0 || stageIndex >= _levelConfig.StageConfigs.Count)
                return null;

            if (ResolveStageConfig(stageIndex) is ClearAllEnemiesStageConfig clearAllEnemiesStageConfig == false)
                return null;

            return ClearAllEnemiesWaveRuntimeData.FromConfig(clearAllEnemiesStageConfig);
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
            if (entity != null && _spawnedTemporaryEntities.Contains(entity) == false)
                _spawnedTemporaryEntities.Add(entity);
        }

        public void StartCurrent()
        {
            _stageEndedDisposable = _currentStage.Completed.Subscribe(OnStageCompleted);
            _currentStage.Start();
        }

        public void UpdateCurrent(float deltaTime) => _currentStage.Update(deltaTime);

        public void CleanupCurrent() => _currentStage.Cleanup();

        public void Dispose()
        {
            _currentStage?.Dispose();
            _stageEndedDisposable?.Dispose();
        }

        private void OnStageCompleted()
        {
            _currentStageResult.Value = StageResults.Completed;
            ClearTemporaryEntities();
            _stageCompleted.Invoke();
        }

        private void ClearTemporaryEntities()
        {
            foreach (Entity entity in _spawnedTemporaryEntities)
                _entitiesLifeContext.Release(entity);

            _spawnedTemporaryEntities.Clear();
        }

        private void ValidateStageConfigs()
        {
            for (int stageIndex = 0; stageIndex < _levelConfig.StageConfigs.Count; stageIndex++)
            {
                if (_levelConfig.StageConfigs[stageIndex] != null)
                    continue;

                throw new InvalidOperationException(
                    $"Stage config for wave {stageIndex + 1} is missing in LevelConfig.");
            }
        }

        private StageConfig ResolveStageConfig(int stageIndex)
        {
            if (stageIndex < 0 || stageIndex >= _levelConfig.StageConfigs.Count)
            {
                throw new InvalidOperationException(
                    $"Wave index {stageIndex} is out of range for level with {_levelConfig.StageConfigs.Count} stages.");
            }

            StageConfig stageConfig = _levelConfig.StageConfigs[stageIndex];

            if (stageConfig == null)
            {
                throw new InvalidOperationException(
                    $"Stage config for wave {stageIndex + 1} is missing in LevelConfig.");
            }

            return stageConfig;
        }
    }
}
