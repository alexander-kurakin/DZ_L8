using System;
using System.Collections.Generic;
using Assets._Project.Develop.Runtime.Configs.Meta.Stats;
using Assets._Project.Develop.Runtime.UI;
using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.UI.Stats;
using Assets._Project.Develop.Runtime.UI.Wallet;
using UnityEngine;

namespace _Project.Develop.Runtime.UI.Gameplay
{
    public class GameplayScreenPresenter : IPresenter
    {
        //logic
        private readonly ProjectPresentersFactory _projectPresentersFactory;
        
        //view
        private readonly GameplayScreenView _screen;
        
        private readonly List<IPresenter> _childPresenters = new();

        public GameplayScreenPresenter(
            ProjectPresentersFactory projectPresentersFactory,
            GameplayScreenView screen)
        {
            _projectPresentersFactory = projectPresentersFactory;
            _screen = screen;
        }

        public void Initialize()
        {
            CreateWallet();
            CreateStats();
            
            foreach (IPresenter presenter in _childPresenters)
            {
                presenter.Initialize();
            }
        }

        public void Dispose()
        {
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
        
    }
}