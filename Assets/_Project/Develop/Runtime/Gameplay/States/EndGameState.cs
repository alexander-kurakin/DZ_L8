using Assets._Project.Develop.Runtime.Configs.Meta.Stats;
using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;
using Assets._Project.Develop.Runtime.Utilities.CoroutinesManagment;
using Assets._Project.Develop.Runtime.Utilities.DataManagment.DataProviders;
using Assets._Project.Develop.Runtime.Utilities.SceneManagment;
using Assets._Project.Develop.Runtime.Utilities.StateMachineCore;
using DG.Tweening;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.States
{
    public abstract class EndGameState : State, IUpdatableState
    {
        private const float END_GAME_BEAT_DELAY_SECONDS = 1.2f;

        private readonly IInputService _inputService;
        private readonly PlayerDataProvider _playerDataProvider;
        private readonly SceneSwitcherService _sceneSwitcherService;
        private readonly ICoroutinesPerformer _coroutinesPerformer;
        private readonly IMouseInputService _mouseInputService;
        private readonly StatsService _statsService;

        private Tween _endGameBeatTween;

        protected EndGameState(
            IInputService inputService,
            PlayerDataProvider playerDataProvider,
            SceneSwitcherService sceneSwitcherService,
            ICoroutinesPerformer coroutinesPerformer,
            StatsService statsService,
            IMouseInputService mouseInputService)
        {
            _inputService = inputService;
            _playerDataProvider = playerDataProvider;
            _sceneSwitcherService = sceneSwitcherService;
            _coroutinesPerformer = coroutinesPerformer;
            _statsService = statsService;
            _mouseInputService = mouseInputService;
        }
        
        public override void Enter()
        {
            base.Enter();

            _inputService.IsEnabled = false;
            _mouseInputService.ShowCursor();

            _endGameBeatTween?.Kill();
            _endGameBeatTween = DOVirtual
                .DelayedCall(END_GAME_BEAT_DELAY_SECONDS, CompleteEndGameEnter)
                .SetUpdate(true)
                .Play();
        }

        public override void Exit()
        {
            _endGameBeatTween?.Kill();
            _endGameBeatTween = null;

            base.Exit();

            _inputService.IsEnabled = true;
        }
        
        protected StatsService Stats => _statsService;

        protected virtual void OnEndGameStateEntered()
        {
        }

        protected abstract void RecordResults();

        private void CompleteEndGameEnter()
        {
            _endGameBeatTween = null;
            OnEndGameStateEntered();
            RecordResults();
            SaveAllData();
        }

        private void SaveAllData()
            => _coroutinesPerformer.StartPerform(_playerDataProvider.SaveAsync());

        private void SwitchToMainMenu() 
            => _coroutinesPerformer.StartPerform(_sceneSwitcherService.ProcessSwitchTo(Scenes.MainMenu));
        
        public void Update(float deltaTime)
        {
            if (Input.GetKeyDown(KeyCode.Q))
                SwitchToMainMenu();
        }
    }
}
