using System;
using System.Collections.Generic;
using _Project.Develop.Runtime.Gameplay.Features.InputFeature;
using _Project.Develop.Runtime.UI.Gameplay.Abilities;
using Assets._Project.Develop.Runtime.Configs.Meta.Stats;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.GameplayStateBridge;
using Assets._Project.Develop.Runtime.Gameplay.Features.MainHero;
using Assets._Project.Develop.Runtime.Gameplay.Features.SpellcoreProgressionFeature;
using Assets._Project.Develop.Runtime.UI;
using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.UI.Gameplay.HealthDisplay;
using Assets._Project.Develop.Runtime.UI.Gameplay.Stages;
using Assets._Project.Develop.Runtime.UI.Stats;
using _Project.Develop.Runtime.UI.Gameplay.Essence;
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
        private SpellcoreProgressionService _spellcoreProgressionService;
        private AbilityListPresenter _abilityListPresenter;

        private IDisposable _mainHeroRegisteredDisposable;
        private IDisposable _gameplayStateChangedDisposable;

        public GameplayScreenPresenter(
            ProjectPresentersFactory projectPresentersFactory,
            GameplayScreenView screen,
            GameplayPresentersFactory gameplayPresentersFactory,
            MainHeroHolderService mainHeroHolderService,
            SpellcoreProgressionService spellcoreProgressionService)
        {
            _projectPresentersFactory = projectPresentersFactory;
            _screen = screen;
            _gameplayPresentersFactory = gameplayPresentersFactory;
            _mainHeroHolderService = mainHeroHolderService;
            _spellcoreProgressionService = spellcoreProgressionService;
        }

        public void Initialize()
        {
            CreateCurrencyHud();
            CreateStats();
            CreateStageNumber();
            CreateWavePreview();
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
            _spellcoreProgressionService.Changed += OnProgressionChanged;
            UpdateAbilityBarVisibility(mainHero.GameplayPhase.Value);
        }

        private void OnProgressionChanged()
        {
            if (_mainHeroHolderService.MainHero == null)
                return;

            UpdateAbilityBarVisibility(_mainHeroHolderService.MainHero.GameplayPhase.Value);
        }

        private void OnGameplayStateChanged(GameplayStates oldState, GameplayStates newState)
        {
            UpdateAbilityBarVisibility(newState);
        }

        private void UpdateAbilityBarVisibility(GameplayStates gameplayState)
        {
            if (_abilityListPresenter == null)
                return;

            if (_spellcoreProgressionService.HasAnyPlantAbilityUnlocked())
                _abilityListPresenter.ShowAll();
            else
                _abilityListPresenter.HideAll();
        }

        public void Dispose()
        {
            foreach (IPresenter presenter in _childPresenters)
                presenter.Dispose();

            _childPresenters.Clear();
            
            _mainHeroRegisteredDisposable?.Dispose();
            _gameplayStateChangedDisposable?.Dispose();
            _spellcoreProgressionService.Changed -= OnProgressionChanged;
        }
        
        public void LateUpdate()
        {
            _entitiesHealthDisplayPresenter.LateUpdate();
        }
        
        private void CreateCurrencyHud()
        {
            GameplayCurrencyHudPresenter currencyHudPresenter =
                _gameplayPresentersFactory.CreateGameplayCurrencyHudPresenter(_screen.WalletIconTextListView);

            _childPresenters.Add(currencyHudPresenter);
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

        private void CreateWavePreview()
        {
            WavePreviewPresenter wavePreviewPresenter =
                _gameplayPresentersFactory.CreateWavePreviewPresenter(_screen.WavePreviewView);

            _childPresenters.Add(wavePreviewPresenter);
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