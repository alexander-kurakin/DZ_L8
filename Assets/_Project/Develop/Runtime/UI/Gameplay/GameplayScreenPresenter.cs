using System;
using System.Collections.Generic;
using _Project.Develop.Runtime.Configs.Meta.Stats;
using Assets._Project.Develop.Runtime.UI;
using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.UI.Wallet;

namespace _Project.Develop.Runtime.UI.Gameplay
{
    public class GameplayScreenPresenter : IPresenter
    {
        //logic
        private readonly StatsService _statsService;
        private readonly ProjectPresentersFactory _projectPresentersFactory;
        
        //view
        private readonly GameplayScreenView _screen;
        
        private IDisposable _winsChanged;
        private IDisposable _lossesChanged;
        
        private readonly List<IPresenter> _childPresenters = new();

        public GameplayScreenPresenter(
            ProjectPresentersFactory projectPresentersFactory,
            StatsService statsService,
            GameplayScreenView screen)
        {
            _projectPresentersFactory = projectPresentersFactory;
            _screen = screen;
            _statsService = statsService;
        }

        public void Initialize()
        {
           
            _winsChanged = _statsService.Wins.Subscribe(OnWinsChanged);
            _lossesChanged = _statsService.Losses.Subscribe(OnLossesChanged);
            
            CreateWallet();
            RefreshStatsView();
            
            foreach (IPresenter presenter in _childPresenters)
            {
                presenter.Initialize();
            }
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
        
    }
}