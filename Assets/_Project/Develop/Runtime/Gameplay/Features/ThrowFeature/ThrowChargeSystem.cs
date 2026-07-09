using Assets._Project.Develop.Runtime.Configs.Gameplay.Throw;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;
using Assets._Project.Develop.Runtime.Utilities.Conditions;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.ThrowFeature
{
    public class ThrowChargeSystem : IInitializableSystem, IUpdatableSystem
    {
        private readonly IMouseInputService _mouseInput;
        private readonly ThrowChargeConfig _config;

        private ReactiveVariable<bool> _isChargingThrow;
        private ReactiveVariable<float> _throwChargePower;
        private ICompositeCondition _canChargeThrow;

        private float _chargeElapsed;

        public ThrowChargeSystem(IMouseInputService mouseInput, ThrowChargeConfig config)
        {
            _mouseInput = mouseInput;
            _config = config;
        }

        public void OnInit(Entity entity)
        {
            _isChargingThrow = entity.IsChargingThrow;
            _throwChargePower = entity.ThrowChargePower;
            _canChargeThrow = entity.CanChargeThrow;
        }

        public void OnUpdate(float deltaTime)
        {
            if (_mouseInput.FireButtonDown && _canChargeThrow.Evaluate())
            {
                _isChargingThrow.Value = true;
                _chargeElapsed = 0f;
                _throwChargePower.Value = 0f;
                return;
            }

            if (_isChargingThrow.Value == false)
                return;

            if (_mouseInput.FireButtonHeld == false)
            {
                if (_mouseInput.FireButtonUp == false)
                    ResetCharge();

                return;
            }

            _chargeElapsed += deltaTime;

            float chargeRatio = Mathf.Clamp(_chargeElapsed / _config.MaxChargeTimeSeconds, 0f, 1f);
            float evaluatedRatio = _config.EvaluateChargeRatio(chargeRatio);
            _throwChargePower.Value = Mathf.Lerp(_config.MinThrowPower, _config.MaxThrowPower, evaluatedRatio);
        }

        private void ResetCharge()
        {
            _isChargingThrow.Value = false;
            _chargeElapsed = 0f;
            _throwChargePower.Value = 0f;
        }
    }
}
