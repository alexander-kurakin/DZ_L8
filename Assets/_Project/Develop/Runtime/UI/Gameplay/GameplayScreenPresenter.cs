using System.Collections.Generic;
using _Project.Develop.Runtime.UI.Gameplay.Abilities;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature;
using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.UI.Gameplay.Stages;
using Assets._Project.Develop.Runtime.UI.Stats;

namespace _Project.Develop.Runtime.UI.Gameplay
{
    public class GameplayScreenPresenter : IPresenter
    {
        private readonly GameplayPresentersFactory _gameplayPresentersFactory;
        private readonly GameplayScreenView _screen;
        private readonly PlayerModifiersHolderService _playerModifiersHolderService;

        private readonly List<IPresenter> _childPresenters = new();

        private ModifierListPresenter _modifierListPresenter;

        private System.IDisposable _playerRegisteredDisposable;
        private bool _isInitialized;

        public GameplayScreenPresenter(
            GameplayScreenView screen,
            GameplayPresentersFactory gameplayPresentersFactory,
            PlayerModifiersHolderService playerModifiersHolderService)
        {
            _screen = screen;
            _gameplayPresentersFactory = gameplayPresentersFactory;
            _playerModifiersHolderService = playerModifiersHolderService;
        }

        public void Initialize()
        {
            if (_isInitialized)
                return;

            _isInitialized = true;

            CreateStatsView();
            CreateStageNumber();

            _playerRegisteredDisposable = _playerModifiersHolderService.HeroRegistred.Subscribe(OnPlayerRegistered);

            if (_playerModifiersHolderService.PlayerEntity != null)
                OnPlayerRegistered(_playerModifiersHolderService.PlayerEntity);

            foreach (IPresenter presenter in _childPresenters)
                presenter.Initialize();
        }

        private void OnPlayerRegistered(Entity playerEntity)
        {
            //CreateModifiers(playerEntity);
        }

        public void Dispose()
        {
            foreach (IPresenter presenter in _childPresenters)
                presenter.Dispose();

            _childPresenters.Clear();
            _playerRegisteredDisposable?.Dispose();
        }

        private void CreateStatsView()
        {
            GameplayStatsPresenter gameplayStatsPresenter =
                _gameplayPresentersFactory.CreateGameplayStatsPresenter(_screen.StatsIconTextListView);

            _childPresenters.Add(gameplayStatsPresenter);
        }

        private void CreateStageNumber()
        {
            StagePresenter stagePresenter = _gameplayPresentersFactory.CreateStagePresenter(_screen.StageNumberView);
            _childPresenters.Add(stagePresenter);
        }
    }
}
