using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.MainHero;
using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System;

namespace _Project.Develop.Runtime.UI.Gameplay.ThrowCrosshair
{
    public class ThrowCrosshairPresenter : ILateUpdatablePresenter, IPresenter
    {
        private readonly ThrowCrosshairView _view;
        private readonly IMouseInputService _mouseInput;
        private readonly MainHeroHolderService _mainHeroHolderService;

        private Entity _heroEntity;
        private IReadOnlyVariable<bool> _isChargingThrow;
        private IDisposable _heroRegisteredDisposable;
        private IDisposable _isChargingThrowDisposable;

        public ThrowCrosshairPresenter(
            ThrowCrosshairView view,
            IMouseInputService mouseInput,
            MainHeroHolderService mainHeroHolderService)
        {
            _view = view;
            _mouseInput = mouseInput;
            _mainHeroHolderService = mainHeroHolderService;
        }

        public void Initialize()
        {
            _view.SetVisible(false);

            if (_mainHeroHolderService.MainHero != null)
                BindHero(_mainHeroHolderService.MainHero);

            _heroRegisteredDisposable = _mainHeroHolderService.HeroRegistred.Subscribe(BindHero);
        }

        public void Dispose()
        {
            _heroRegisteredDisposable?.Dispose();
            _isChargingThrowDisposable?.Dispose();
            _view.SetVisible(false);
        }

        public void LateUpdate()
        {
            if (_view.IsVisible == false)
                return;

            _view.SetScreenPosition(_mouseInput.PointerScreenPosition);
        }

        private void BindHero(Entity heroEntity)
        {
            _isChargingThrowDisposable?.Dispose();

            _heroEntity = heroEntity;
            _isChargingThrow = _heroEntity.IsChargingThrow;
            _isChargingThrowDisposable = _isChargingThrow.Subscribe(OnIsChargingThrowChanged);

            UpdateVisibility();
        }

        private void OnIsChargingThrowChanged(bool oldValue, bool isChargingThrow)
        {
            UpdateVisibility();
        }

        private void UpdateVisibility()
        {
            bool isVisible = _isChargingThrow != null && _isChargingThrow.Value;
            _view.SetVisible(isVisible);

            if (isVisible)
                _view.SetScreenPosition(_mouseInput.PointerScreenPosition);
        }
    }
}
