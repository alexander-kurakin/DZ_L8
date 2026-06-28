using Assets._Project.Develop.Runtime.Configs.Gameplay.Levels;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System;
using System.Collections.Generic;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Stages;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature
{
    public class StageProviderService : IDisposable
    {
        private readonly ReactiveEvent _stageCompleted = new();
        private ReactiveVariable<int> _currentStageNumber = new();
        private ReactiveVariable<StageResults> _currentStageResult = new();

        private LevelConfig _levelConfig;
        private StagesFactory _stagesFactory;

        private IStage _currentStage;
        
        private List<Entity> _spawnedTemporaryEntities = new();
        private EntitiesLifeContext _entitiesLifeContext;

        private IDisposable _stageEndedDisposable;

        public StageProviderService(
            LevelConfig levelConfig, 
            StagesFactory stagesFactory,
            EntitiesLifeContext entitiesLifeContext)
        {
            _levelConfig = levelConfig;
            _stagesFactory = stagesFactory;
            _entitiesLifeContext = entitiesLifeContext;
        }

        public IReadOnlyVariable<int> CurrentStageNumber => _currentStageNumber;
        public IReadOnlyVariable<StageResults> CurrentStageResult => _currentStageResult;
        public IReadOnlyEvent StageCompleted => _stageCompleted;
        
        public int CurrentStageEnemiesCount
        {
            get
            {
                int stageIndex = _currentStageNumber.Value - 1;
                
                if (stageIndex < 0 || stageIndex >= _levelConfig.StageConfigs.Count)
                    return 0;
                
                if (_levelConfig.StageConfigs[stageIndex] is ClearAllEnemiesStageConfig clearAllEnemiesStageConfig)
                    return clearAllEnemiesStageConfig.EnemiesCount;
                
                return 0;
            }
        }

        public int StagesCount => _levelConfig.StageConfigs.Count;

        public IReadOnlyList<WaveEnemyPreviewType> GetUpcomingWaveEnemyPreviewTypes()
        {
            int upcomingStageIndex = _currentStageNumber.Value;

            if (upcomingStageIndex < 0 || upcomingStageIndex >= _levelConfig.StageConfigs.Count)
                return Array.Empty<WaveEnemyPreviewType>();

            if (_levelConfig.StageConfigs[upcomingStageIndex] is ClearAllEnemiesStageConfig clearAllEnemiesStageConfig == false)
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

        public bool HasNextStage() => CurrentStageNumber.Value < StagesCount;

        public void SwitchToNext()
        {
            if (HasNextStage() == false)
                throw new InvalidOperationException();

            if (_currentStage != null)
                CleanupCurrent();

            _currentStageNumber.Value++;
            _currentStageResult.Value = StageResults.Uncompleted;

            _currentStage = _stagesFactory.Create(_levelConfig.StageConfigs[_currentStageNumber.Value - 1]);
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
    }
}
