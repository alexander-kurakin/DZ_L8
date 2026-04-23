using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.UI.Wallet;
using System;
using System.Collections.Generic;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Levels;
using Assets._Project.Develop.Runtime.Configs.Meta.Stats;
using Assets._Project.Develop.Runtime.Gameplay.Infrastructure;
using Assets._Project.Develop.Runtime.UI.Stats;
using Assets._Project.Develop.Runtime.Utilities.CoroutinesManagment;
using Assets._Project.Develop.Runtime.Utilities.SceneManagment;

namespace Assets._Project.Develop.Runtime.UI.MainMenu
{
    public class MainMenuScreenPresenter : IPresenter
    {
        private readonly MainMenuScreenView _screen;

        private readonly ProjectPresentersFactory _projectPresentersFactory;

        private readonly List<IPresenter> _childPresenters = new();
        
        private readonly LevelsListConfig _levelsListConfig;
        
        private readonly ICoroutinesPerformer _coroutinesPerformer;
        private readonly SceneSwitcherService _sceneSwitcherService;

        public MainMenuScreenPresenter(
            MainMenuScreenView screen,
            ProjectPresentersFactory projectPresentersFactory,
            LevelsListConfig levelsListConfig,
            ICoroutinesPerformer coroutinesPerformer,
            SceneSwitcherService sceneSwitcherService
            )
        {
            _screen = screen;
            _projectPresentersFactory = projectPresentersFactory;
            _levelsListConfig = levelsListConfig;
            _coroutinesPerformer = coroutinesPerformer;
            _sceneSwitcherService = sceneSwitcherService;
        }

        public void Initialize()
        {
            _screen.PlayButtonClicked += OnPlayButtonClicked;

            CreateWallet();
            CreateStats();

            foreach (IPresenter presenter in _childPresenters)
                presenter.Initialize();
        }
        
        public void Dispose()
        {
            _screen.PlayButtonClicked -= OnPlayButtonClicked;

            foreach (IPresenter presenter in _childPresenters)
                presenter.Dispose();

            _childPresenters.Clear();
        }

        private void CreateWallet()
        {
            WalletPresenter walletPresenter = _projectPresentersFactory.CreateWalletPresenter(_screen.WalletIconTextListView);
            _childPresenters.Add(walletPresenter);
        }

        private void CreateStats()
        {
            GameStatsPresenter gameStatsPresenter = _projectPresentersFactory.CreateGameStatsPresenter(_screen.StatsIconTextListView);
            _childPresenters.Add(gameStatsPresenter);            
        }

        private void OnPlayButtonClicked()
        {
            int randomLevel = _levelsListConfig.GetRandomLevelNumber();
            _coroutinesPerformer.StartPerform(_sceneSwitcherService.ProcessSwitchTo(Scenes.Gameplay, new GameplayInputArgs(randomLevel)));
        }
    }
}
