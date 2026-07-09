using Assets._Project.Develop.Runtime.Configs.Gameplay.Throw;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;
using Assets._Project.Develop.Runtime.Utilities.Conditions;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.ThrowFeature
{
    public class ThrowReleaseSystem : IInitializableSystem, IUpdatableSystem
    {
        private readonly IMouseInputService _mouseInput;
        private readonly IMouseRaycastService _mouseRaycastService;
        private readonly ThrowChargeConfig _throwChargeConfig;

        private ReactiveVariable<bool> _isChargingThrow;
        private ReactiveVariable<float> _throwChargePower;
        private ReactiveVariable<bool> _isProjectileInHand;
        private ReactiveVariable<Entity> _currentProjectile;
        private Transform _heroTransform;
        private Transform _throwReleasePoint;
        private ReactiveEvent<ThrowReleaseData> _throwReleased;
        private ICompositeCondition _canReleaseThrow;

        public ThrowReleaseSystem(
            IMouseInputService mouseInput,
            IMouseRaycastService mouseRaycastService,
            ThrowChargeConfig throwChargeConfig)
        {
            _mouseInput = mouseInput;
            _mouseRaycastService = mouseRaycastService;
            _throwChargeConfig = throwChargeConfig;
        }

        public void OnInit(Entity entity)
        {
            _isChargingThrow = entity.IsChargingThrow;
            _throwChargePower = entity.ThrowChargePower;
            _isProjectileInHand = entity.IsProjectileInHand;
            _currentProjectile = entity.CurrentProjectile;
            _heroTransform = entity.Transform;
            _throwReleasePoint = entity.ThrowReleasePoint;
            _throwReleased = entity.ThrowReleased;
            _canReleaseThrow = entity.CanReleaseThrow;
        }

        public void OnUpdate(float deltaTime)
        {
            if (_mouseInput.FireButtonUp == false)
                return;

            if (_isChargingThrow.Value == false)
                return;

            if (_canReleaseThrow.Evaluate() == false)
            {
                _isChargingThrow.Value = false;
                _throwChargePower.Value = 0f;
                return;
            }

            float power = _throwChargePower.Value;
            Vector2 pointerScreenPosition = _mouseInput.PointerScreenPosition;
            bool hasAimPoint = _mouseRaycastService.TryGetThrowAimPointFromScreen(
                pointerScreenPosition,
                _heroTransform,
                out Vector3 aimPoint);

            Vector3 direction;

            if (_mouseRaycastService.TryGetThrowDirectionFromCamera(pointerScreenPosition, out Vector3 cameraDirection) == false)
                direction = _throwReleasePoint.forward;
            else
                direction = _throwChargeConfig.ResolveThrowDirection(cameraDirection, power);

            Entity projectile = _currentProjectile.Value;

            _throwReleased.Invoke(new ThrowReleaseData(power, direction, aimPoint, hasAimPoint, projectile));

            _isProjectileInHand.Value = false;
            _isChargingThrow.Value = false;
            _throwChargePower.Value = 0f;
        }
    }
}
