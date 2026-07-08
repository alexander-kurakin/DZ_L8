using Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.WaveProgressFeature;
using Assets._Project.Develop.Runtime.UI.CommonViews;
using Assets._Project.Develop.Runtime.UI.Core;
using System;

namespace Assets._Project.Develop.Runtime.UI.Gameplay.Stages
{
    public class StagePresenter : IPresenter
    {
        private readonly IconTextView _view;
        private readonly StageProviderService _stageProviderService;
        private readonly GameplayPhaseService _gameplayPhaseService;
        private readonly WaveProgressService _waveProgressService;

        private IDisposable _currentStageNumberChangedDisposable;
        private IDisposable _currentStageResultChangedDisposable;
        private IDisposable _gameplayPhaseChangedDisposable;

        public StagePresenter(
            IconTextView view,
            StageProviderService stageProviderService,
            GameplayPhaseService gameplayPhaseService,
            WaveProgressService waveProgressService)
        {
            _view = view;
            _stageProviderService = stageProviderService;
            _gameplayPhaseService = gameplayPhaseService;
            _waveProgressService = waveProgressService;
        }

        public void Initialize()
        {
            _currentStageNumberChangedDisposable = _stageProviderService.CurrentStageNumber.Subscribe(OnStageDataChanged);
            _currentStageResultChangedDisposable = _stageProviderService.CurrentStageResult.Subscribe(OnStageResultChanged);
            _gameplayPhaseChangedDisposable = _gameplayPhaseService.CurrentPhase.Subscribe(OnGameplayPhaseChanged);
            _waveProgressService.Changed += OnProgressionChanged;

            UpdateStageNumber();
        }

        public void Dispose()
        {
            _currentStageNumberChangedDisposable?.Dispose();
            _currentStageResultChangedDisposable?.Dispose();
            _gameplayPhaseChangedDisposable?.Dispose();
            _waveProgressService.Changed -= OnProgressionChanged;
        }

        private void OnStageDataChanged(int oldValue, int newValue) => UpdateStageNumber();

        private void OnStageResultChanged(StageResults oldValue, StageResults newValue) => UpdateStageNumber();

        private void OnProgressionChanged() => UpdateStageNumber();

        private void OnGameplayPhaseChanged(GameplayStates oldState, GameplayStates newState) => UpdateStageNumber();

        private void UpdateStageNumber()
        {
            int displayWave = IsPreparationPhase()
                ? _waveProgressService.CompletedWaves
                : _stageProviderService.CurrentStageNumber.Value;

            int stagesCount = _stageProviderService.StagesCount;
            _view.SetText($"{displayWave} / {stagesCount}");
        }

        private bool IsPreparationPhase() =>
            _gameplayPhaseService.CurrentPhase.Value == GameplayStates.Preparation;
    }
}
