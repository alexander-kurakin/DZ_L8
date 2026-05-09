using Assets._Project.Develop.Runtime.Configs.Gameplay.Levels;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System;
using System.Collections.Generic;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature
{
    public class StageProviderService : IDisposable
    {
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

        public int StagesCount => _levelConfig.StageConfigs.Count;

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
