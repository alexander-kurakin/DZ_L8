using Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.WaveProgressFeature;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using Assets._Project.Develop.Runtime.Utilities.StateMachineCore;
using System;

namespace Assets._Project.Develop.Runtime.Gameplay.States
{
    public class StageProcessState : State, IUpdatableState, IDisposable
    {
        private readonly StageProviderService _stageProviderService;
        private readonly GameplayPhaseService _gameplayPhaseService;
        private readonly WaveProgressService _waveProgressService;

        private IDisposable _stageCompletedSubscription;

        public StageProcessState(
            StageProviderService stageProviderService,
            GameplayPhaseService gameplayPhaseService,
            WaveProgressService waveProgressService)
        {
            _stageProviderService = stageProviderService;
            _gameplayPhaseService = gameplayPhaseService;
            _waveProgressService = waveProgressService;
        }

        public override void Enter()
        {
            base.Enter();

            _gameplayPhaseService.SetPhase(GameplayStates.Search);
            _stageCompletedSubscription = _stageProviderService.StageCompleted.Subscribe(OnStageCompleted);
            StartCurrentStage();
        }

        public void Update(float deltaTime)
        {
            _stageProviderService.UpdateCurrent(deltaTime);
        }

        public override void Exit()
        {
            base.Exit();

            _stageCompletedSubscription?.Dispose();
            _stageCompletedSubscription = null;
            _stageProviderService.CleanupCurrent();
        }

        public void Dispose()
        {
            _stageCompletedSubscription?.Dispose();
        }

        private void StartCurrentStage()
        {
            _stageProviderService.SwitchToNext();
            _stageProviderService.StartCurrent();
            _waveProgressService.OnCombatWaveEntered(_stageProviderService.CurrentStageNumber.Value);
        }

        private void OnStageCompleted()
        {
            if (_stageProviderService.HasNextStage() == false)
                return;

            _stageProviderService.CleanupCurrent();
            _stageProviderService.SwitchToNext();
            _stageProviderService.StartCurrent();
            _waveProgressService.OnCombatWaveEntered(_stageProviderService.CurrentStageNumber.Value);
        }
    }
}
