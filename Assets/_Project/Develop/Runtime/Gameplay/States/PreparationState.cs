using Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.WaveProgressFeature;
using Assets._Project.Develop.Runtime.Utilities.Audio;
using Assets._Project.Develop.Runtime.Utilities.StateMachineCore;

namespace Assets._Project.Develop.Runtime.Gameplay.States
{
    public class PreparationState : State, IUpdatableState
    {
        private readonly PreparationTriggerService _preparationTriggerService;
        private readonly GameplayPhaseService _gameplayPhaseService;
        private readonly WaveProgressService _waveProgressService;
        private readonly IBackgroundMusicService _backgroundMusicService;

        public PreparationState(
            PreparationTriggerService preparationTriggerService,
            GameplayPhaseService gameplayPhaseService,
            WaveProgressService waveProgressService,
            IBackgroundMusicService backgroundMusicService)
        {
            _preparationTriggerService = preparationTriggerService;
            _gameplayPhaseService = gameplayPhaseService;
            _waveProgressService = waveProgressService;
            _backgroundMusicService = backgroundMusicService;
        }

        public override void Enter()
        {
            base.Enter();

            _preparationTriggerService.Create();
            _gameplayPhaseService.SetPhase(GameplayStates.Preparation);
            _waveProgressService.OnPreparationEntered();
            _backgroundMusicService.Play(BackgroundMusicTrackIDs.Preparation);
        }

        public void Update(float deltaTime)
        {
            _preparationTriggerService.Update(deltaTime);
        }

        public override void Exit()
        {
            base.Exit();
            _preparationTriggerService.Cleanup();
        }
    }
}
