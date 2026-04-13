using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.UI.Wallet;
using System;
using System.Collections.Generic;
using _Project.Develop.Runtime.Configs.Meta.Stats;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Levels;
using Assets._Project.Develop.Runtime.Gameplay.Infrastructure;
using Assets._Project.Develop.Runtime.Utilities.CoroutinesManagment;
using Assets._Project.Develop.Runtime.Utilities.SceneManagment;
using UnityEngine;

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

        private readonly StatsService _statsService;
        
        private IDisposable _winsChanged;
        private IDisposable _lossesChanged;

        public MainMenuScreenPresenter(
            MainMenuScreenView screen,
            ProjectPresentersFactory projectPresentersFactory,
            LevelsListConfig levelsListConfig,
            ICoroutinesPerformer coroutinesPerformer,
            SceneSwitcherService sceneSwitcherService,
            StatsService statsService
            )
        {
            _screen = screen;
            _projectPresentersFactory = projectPresentersFactory;
            _levelsListConfig = levelsListConfig;
            _coroutinesPerformer = coroutinesPerformer;
            _sceneSwitcherService = sceneSwitcherService;
            _statsService = statsService;
        }

        public void Initialize()
        {
            _screen.PlayButtonClicked += OnPlayButtonClicked;

            _winsChanged = _statsService.Wins.Subscribe(OnWinsChanged);
            _lossesChanged = _statsService.Losses.Subscribe(OnLossesChanged);

            CreateWallet();
            RefreshStatsView();

            foreach (IPresenter presenter in _childPresenters)
                presenter.Initialize();
        }
        
        private void OnLossesChanged(int arg1, int arg2)
        {
            RefreshStatsView();
        }

        private void OnWinsChanged(int arg1, int arg2)
        {
            RefreshStatsView();
        }

        private void RefreshStatsView()
        {
            _screen.StatsView.SetText(_statsService.GetStatsText());
        }

        public void Dispose()
        {
            _screen.PlayButtonClicked -= OnPlayButtonClicked;

            foreach (IPresenter presenter in _childPresenters)
                presenter.Dispose();

            _childPresenters.Clear();
            
            _winsChanged?.Dispose();
            _lossesChanged?.Dispose();
        }

        private void CreateWallet()
        {
            WalletPresenter walletPresenter = _projectPresentersFactory.CreateWalletPresenter(_screen.WalletView);

            _childPresenters.Add(walletPresenter);
        }

        private void OnPlayButtonClicked()
        {
            int randomLevel = _levelsListConfig.GetRandomLevelNumber();
            _coroutinesPerformer.StartPerform(_sceneSwitcherService.ProcessSwitchTo(Scenes.Gameplay, new GameplayInputArgs(randomLevel)));
        }
    }
}
