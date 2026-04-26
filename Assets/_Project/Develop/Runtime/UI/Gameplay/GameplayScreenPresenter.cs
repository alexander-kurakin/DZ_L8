using System;
using System.Collections.Generic;
using _Project.Develop.Runtime.Gameplay.Features.InputFeature;
using _Project.Develop.Runtime.UI.Gameplay.Abilities;
using Assets._Project.Develop.Runtime.Configs.Meta.Stats;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.GameplayStateBridge;
using Assets._Project.Develop.Runtime.Gameplay.Features.MainHero;
using Assets._Project.Develop.Runtime.UI;
using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.UI.Gameplay.HealthDisplay;
using Assets._Project.Develop.Runtime.UI.Gameplay.Stages;
using Assets._Project.Develop.Runtime.UI.Stats;
using Assets._Project.Develop.Runtime.UI.Wallet;
using UnityEngine;

namespace _Project.Develop.Runtime.UI.Gameplay
{
    public class GameplayScreenPresenter : IPresenter
    {
        //logic
        private readonly ProjectPresentersFactory _projectPresentersFactory;

        private readonly GameplayPresentersFactory _gameplayPresentersFactory;
        //view
        private readonly GameplayScreenView _screen;
        
        private readonly List<IPresenter> _childPresenters = new();
        
        private EntitiesHealthDisplayPresenter _entitiesHealthDisplayPresenter;

        private MainHeroHolderService _mainHeroHolderService;
        private AbilityListPresenter _abilityListPresenter;

        private IDisposable _mainHeroRegisteredDisposable;
        private IDisposable _gameplayStateChangedDisposable;

        public GameplayScreenPresenter(
            ProjectPresentersFactory projectPresentersFactory,
            GameplayScreenView screen,
            GameplayPresentersFactory gameplayPresentersFactory,
            MainHeroHolderService mainHeroHolderService)
        {
            _projectPresentersFactory = projectPresentersFactory;
            _screen = screen;
            _gameplayPresentersFactory = gameplayPresentersFactory;
            _mainHeroHolderService = mainHeroHolderService;
        }

        public void Initialize()
        {
            CreateWallet();
            CreateStats();
            CreateStageNumber();
            CreateEntitiesHealthDisplay();
            
            _mainHeroRegisteredDisposable = _mainHeroHolderService.HeroRegistred.Subscribe(OnMainHeroRegistered);
            
            foreach (IPresenter presenter in _childPresenters)
            {
                presenter.Initialize();
            }
        }

        private void OnMainHeroRegistered(Entity mainHero)
        {
            CreateAbitities(mainHero);
            
            _gameplayStateChangedDisposable = mainHero.GameplayPhase.Subscribe(OnGameplayStateChanged);
        }

        private void OnGameplayStateChanged(GameplayStates oldState, GameplayStates newState)
        {
            if (newState == GameplayStates.StageProcess)
                _abilityListPresenter.HideAll();
            else
                _abilityListPresenter.ShowAll();
        }

        public void Dispose()
        {
            foreach (IPresenter presenter in _childPresenters)
                presenter.Dispose();

            _childPresenters.Clear();
            
            _mainHeroRegisteredDisposable?.Dispose();
            _gameplayStateChangedDisposable?.Dispose();
        }
        
        public void LateUpdate()
        {
            _entitiesHealthDisplayPresenter.LateUpdate();
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

        private void CreateStageNumber()
        {
            StagePresenter stagePresenter = _gameplayPresentersFactory.CreateStagePresenter(_screen.StageNumberView);
            _childPresenters.Add(stagePresenter);
        }

        private void CreateAbitities(Entity mainHero)
        {
            _abilityListPresenter = _gameplayPresentersFactory.CreateAbilityListPresenter(_screen.AbilitiesView, mainHero);
            
            _abilityListPresenter.Initialize();
            _childPresenters.Add(_abilityListPresenter); 
        }

        private void CreateEntitiesHealthDisplay()
        {
            _entitiesHealthDisplayPresenter = _gameplayPresentersFactory.CreateEntitiesHealthDisplayPresenter(_screen.EntitiesHealthDisplay);

            _childPresenters.Add(_entitiesHealthDisplayPresenter);
        }
    }
}