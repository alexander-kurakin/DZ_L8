using Assets._Project.Develop.Runtime.Gameplay.Features.GameplayTimeScale;
using Assets._Project.Develop.Runtime.UI.Core;
using System;
using UnityEngine;

namespace _Project.Develop.Runtime.UI.Gameplay.CombatTimeScale
{
    public class CombatTimeScalePresenter : IPresenter
    {
        private readonly ViewsFactory _viewsFactory;
        private readonly Transform _parent;
        private readonly GameplayTimeScaleService _gameplayTimeScaleService;

        private CombatTimeScaleView _view;

        public CombatTimeScalePresenter(
            ViewsFactory viewsFactory,
            Transform parent,
            GameplayTimeScaleService gameplayTimeScaleService)
        {
            _viewsFactory = viewsFactory;
            _parent = parent;
            _gameplayTimeScaleService = gameplayTimeScaleService;
        }

        public void Initialize()
        {
            _view = _viewsFactory.Create<CombatTimeScaleView>(ViewIDs.CombatTimeScaleView, _parent);
            _view.PauseClicked += OnPauseClicked;
            _view.NormalSpeedClicked += OnNormalSpeedClicked;
            _view.FastSpeedClicked += OnFastSpeedClicked;
            _gameplayTimeScaleService.Changed += OnTimeScaleChanged;

            OnTimeScaleChanged();
        }

        public void Dispose()
        {
            if (_view != null)
            {
                _view.PauseClicked -= OnPauseClicked;
                _view.NormalSpeedClicked -= OnNormalSpeedClicked;
                _view.FastSpeedClicked -= OnFastSpeedClicked;
                _viewsFactory.Release(_view);
                _view = null;
            }

            _gameplayTimeScaleService.Changed -= OnTimeScaleChanged;
        }

        private void OnPauseClicked() => _gameplayTimeScaleService.SetMode(GameplayTimeScaleMode.Paused);

        private void OnNormalSpeedClicked() => _gameplayTimeScaleService.SetMode(GameplayTimeScaleMode.Normal);

        private void OnFastSpeedClicked() => _gameplayTimeScaleService.SetMode(GameplayTimeScaleMode.Fast);

        private void OnTimeScaleChanged()
        {
            if (_view == null)
                return;

            _view.SetSelectedMode(_gameplayTimeScaleService.CurrentMode);
        }
    }
}
