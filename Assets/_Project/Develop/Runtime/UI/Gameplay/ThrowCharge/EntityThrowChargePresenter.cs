using Assets._Project.Develop.Runtime.Configs.Gameplay.Throw;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.UI.CommonViews;
using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System;
using UnityEngine;

namespace _Project.Develop.Runtime.UI.Gameplay.ThrowCharge
{
    public class EntityThrowChargePresenter : IPresenter
    {
        private readonly Entity _entity;
        private readonly Bar _bar;
        private readonly float _maxThrowPower;

        private IReadOnlyVariable<float> _throwChargePower;
        private IReadOnlyVariable<bool> _isChargingThrow;

        private IDisposable _throwChargePowerChangedDisposable;
        private IDisposable _isChargingThrowChangedDisposable;

        public EntityThrowChargePresenter(Entity entity, Bar bar, ThrowChargeConfig throwChargeConfig)
        {
            _entity = entity;
            _bar = bar;
            _maxThrowPower = throwChargeConfig.MaxThrowPower;
        }

        public RectTransform BarRectTransform => (RectTransform)_bar.transform;

        public Bar Bar => _bar;

        public void Initialize()
        {
            _throwChargePower = _entity.ThrowChargePower;
            _isChargingThrow = _entity.IsChargingThrow;

            _throwChargePowerChangedDisposable = _throwChargePower.Subscribe(OnThrowChargePowerChanged);
            _isChargingThrowChangedDisposable = _isChargingThrow.Subscribe(OnIsChargingThrowChanged);

            UpdateBarVisibility();
            UpdateBarFill();
        }

        public void Dispose()
        {
            _throwChargePowerChangedDisposable?.Dispose();
            _isChargingThrowChangedDisposable?.Dispose();
        }

        private void OnThrowChargePowerChanged(float oldPower, float power) => UpdateBarFill();

        private void OnIsChargingThrowChanged(bool oldIsCharging, bool isCharging) => UpdateBarVisibility();

        private void UpdateBarVisibility()
        {
            _bar.gameObject.SetActive(_isChargingThrow.Value);
        }

        private void UpdateBarFill()
        {
            if (_maxThrowPower <= 0f)
                return;

            _bar.UpdateValue(_throwChargePower.Value / _maxThrowPower);
        }
    }
}
