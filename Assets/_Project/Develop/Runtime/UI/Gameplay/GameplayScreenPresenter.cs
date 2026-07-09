using System.Collections.Generic;
using _Project.Develop.Runtime.UI.Gameplay.Abilities;
using _Project.Develop.Runtime.UI.Gameplay.GnomeKillToast;
using _Project.Develop.Runtime.UI.Gameplay.ThrowCharge;
using _Project.Develop.Runtime.UI.Gameplay.ThrowCrosshair;
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
            CreateThrowChargeDisplay();
            CreateThrowCrosshair();
            CreateGnomeKillToast();

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

        private void CreateThrowChargeDisplay()
        {
            EntitiesThrowChargeDisplayPresenter throwChargeDisplayPresenter =
                _gameplayPresentersFactory.CreateEntitiesThrowChargeDisplayPresenter(_screen.EntitiesThrowChargeDisplay);

            _childPresenters.Add(throwChargeDisplayPresenter);
        }

        private void CreateThrowCrosshair()
        {
            ThrowCrosshairPresenter throwCrosshairPresenter =
                _gameplayPresentersFactory.CreateThrowCrosshairPresenter(_screen.ThrowCrosshairView);

            _childPresenters.Add(throwCrosshairPresenter);
        }

        private void CreateGnomeKillToast()
        {
            GnomeKillToastPresenter gnomeKillToastPresenter =
                _gameplayPresentersFactory.CreateGnomeKillToastPresenter(_screen.GnomeKillToastView);

            _childPresenters.Add(gnomeKillToastPresenter);
        }

        public void LateUpdate()
        {
            foreach (IPresenter presenter in _childPresenters)
            {
                if (presenter is ILateUpdatablePresenter lateUpdatablePresenter)
                    lateUpdatablePresenter.LateUpdate();
            }
        }
    }
}
