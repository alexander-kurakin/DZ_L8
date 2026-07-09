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
        private readonly IMouseInputService _mouseInput;
        private readonly IMouseRaycastService _mouseRaycastService;
        private readonly ReactiveVariable<Vector3> _movementDirection;
        private readonly ReactiveVariable<Vector3> _rotationDirection;
        private readonly IReadOnlyVariable<bool> _isChargingThrow;
        private readonly Transform _heroTransform;

        public PlayerInputMovementState(
            Entity entity,
            IInputService inputService,
            IMouseInputService mouseInput,
            IMouseRaycastService mouseRaycastService)
        {
            _inputService = inputService;
            _mouseInput = mouseInput;
            _mouseRaycastService = mouseRaycastService;
            _movementDirection = entity.MoveDirection;
            _rotationDirection = entity.RotationDirection;
            _isChargingThrow = entity.IsChargingThrow;
            _heroTransform = entity.Transform;
        }

        public void Update(float deltaTime)
        {
            _movementDirection.Value = _inputService.Direction;
            ApplyMousePointerRotation();
        }

        private void ApplyMousePointerRotation()
        {
            if (_isChargingThrow.Value == true)
                return;

            if (_mouseInput.IsEnabled == false)
                return;

            Vector2 pointerScreenPosition = _mouseInput.PointerScreenPosition;
            float planeY = _heroTransform.position.y;

            if (_mouseRaycastService.TryGetHorizontalPlaneHit(pointerScreenPosition, planeY, out Vector3 hitPoint) == false)
                return;

            Vector3 direction = hitPoint - _heroTransform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude <= 0f)
                return;

            _rotationDirection.Value = direction.normalized;
        }

        public override void Exit()
        {
            base.Exit();

            _movementDirection.Value = Vector3.zero;
        }
    }
}
