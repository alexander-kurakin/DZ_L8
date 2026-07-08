using Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.WaveProgressFeature;
using Assets._Project.Develop.Runtime.Utilities.StateMachineCore;

namespace Assets._Project.Develop.Runtime.Gameplay.States
{
    public class StageProcessState : State, IUpdatableState
    {
        private readonly StageProviderService _stageProviderService;
        private readonly GameplayPhaseService _gameplayPhaseService;
        private readonly WaveProgressService _waveProgressService;

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

            _stageProviderService.SwitchToNext();
            _stageProviderService.StartCurrent();
            _waveProgressService.OnCombatWaveEntered(_stageProviderService.CurrentStageNumber.Value);
            _gameplayPhaseService.SetPhase(GameplayStates.StageProcess);
        }

        public void Update(float deltaTime)
        {
            _stageProviderService.UpdateCurrent(deltaTime);
        }

        public override void Exit()
        {
            base.Exit();
            _stageProviderService.CleanupCurrent();
        }
    }
}
