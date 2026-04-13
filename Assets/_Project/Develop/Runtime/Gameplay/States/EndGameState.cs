using _Project.Develop.Runtime.Configs.Meta.Stats;
using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;
using Assets._Project.Develop.Runtime.Utilities.CoroutinesManagment;
using Assets._Project.Develop.Runtime.Utilities.DataManagment.DataProviders;
using Assets._Project.Develop.Runtime.Utilities.SceneManagment;
using Assets._Project.Develop.Runtime.Utilities.StateMachineCore;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.States
{
    public abstract class EndGameState : State, IUpdatableState
    {
        private readonly IInputService _inputService;
        private readonly PlayerDataProvider _playerDataProvider;
        private readonly SceneSwitcherService _sceneSwitcherService;
        private readonly ICoroutinesPerformer _coroutinesPerformer;
        private readonly StatsService _statsService;

        protected EndGameState(
            IInputService inputService,
            PlayerDataProvider playerDataProvider,
            SceneSwitcherService sceneSwitcherService,
            ICoroutinesPerformer coroutinesPerformer,
            StatsService statsService)
        {
            _inputService = inputService;
            _playerDataProvider = playerDataProvider;
            _sceneSwitcherService = sceneSwitcherService;
            _coroutinesPerformer = coroutinesPerformer;
            _statsService = statsService;
        }
        
        public override void Enter()
        {
            base.Enter();
            
            _inputService.IsEnabled = false;

            OnEndGameStateEntered();
            RecordResults();
            SaveAllData();
        }

        public override void Exit()
        {
            base.Exit();

            _inputService.IsEnabled = true;
        }
        
        protected StatsService Stats => _statsService;

        protected virtual void OnEndGameStateEntered()
        {
        }

        protected abstract void RecordResults();

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
