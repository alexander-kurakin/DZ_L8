using System;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Waves;
using Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature;
using Assets._Project.Develop.Runtime.Utilities.Reactive;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.WaveProgressFeature
{
    public class WaveProgressService : IDisposable
    {
        private readonly StageProviderService _stageProviderService;
        private readonly WaveProgressConfig _config;

        private int _completedWaves;
        private IDisposable _stageCompletedSubscription;

        public WaveProgressService(StageProviderService stageProviderService, WaveProgressConfig config)
        {
            _stageProviderService = stageProviderService;
            _config = config;
        }

        public event Action Changed;

        public int CompletedWaves => _completedWaves;

        public int UpcomingWaveNumber => _completedWaves + 1;

        public void InitializeForRun()
        {
            _completedWaves = 0;
            _stageCompletedSubscription?.Dispose();
            _stageCompletedSubscription = _stageProviderService.StageCompleted.Subscribe(OnWaveCompleted);
            Changed?.Invoke();
        }

        public void OnPreparationEntered() { }

        public void OnCombatWaveEntered(int waveNumber) { }

        public void Dispose()
        {
            _stageCompletedSubscription?.Dispose();
        }

        private void OnWaveCompleted()
        {
            _completedWaves = _stageProviderService.CurrentStageNumber.Value;
            Changed?.Invoke();
        }
    }
}
