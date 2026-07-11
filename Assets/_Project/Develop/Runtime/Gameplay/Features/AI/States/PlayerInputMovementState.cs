using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using Assets._Project.Develop.Runtime.Utilities.StateMachineCore;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.AI.States
{
    public class PlayerInputMovementState : State, IUpdatableState
    {
        private readonly IInputService _inputService;
        private readonly ReactiveVariable<Vector3> _movementDirection;
        private readonly ReactiveVariable<Vector3> _rotationDirection;
        private readonly IReadOnlyVariable<bool> _isChargingThrow;
        private readonly IReadOnlyVariable<bool> _isWatchingThrownProjectile;
        private readonly IReadOnlyVariable<float> _throwPostImpactAimLockRemainingTime;

        public PlayerInputMovementState(Entity entity, IInputService inputService)
        {
            _inputService = inputService;
            _movementDirection = entity.MoveDirection;
            _rotationDirection = entity.RotationDirection;
            _isChargingThrow = entity.IsChargingThrow;
            _isWatchingThrownProjectile = entity.IsWatchingThrownProjectile;
            _throwPostImpactAimLockRemainingTime = entity.ThrowPostImpactAimLockRemainingTime;
        }

        public void Update(float deltaTime)
        {
            Vector3 movementDirection = _inputService.Direction;
            _movementDirection.Value = movementDirection;
            ApplyMovementRotation(movementDirection);
        }

        private void ApplyMovementRotation(Vector3 movementDirection)
        {
            if (CanApplyRotation() == false)
                return;

            movementDirection.y = 0f;

            if (movementDirection.sqrMagnitude <= 0f)
                return;

            _rotationDirection.Value = movementDirection.normalized;
        }

        private bool CanApplyRotation()
        {
            if (_isChargingThrow.Value == true)
                return false;

            if (_isWatchingThrownProjectile.Value == true)
                return false;

            if (_throwPostImpactAimLockRemainingTime.Value > 0f)
                return false;

            return true;
        }

        public override void Exit()
        {
            base.Exit();

            _movementDirection.Value = Vector3.zero;
        }
    }
}
