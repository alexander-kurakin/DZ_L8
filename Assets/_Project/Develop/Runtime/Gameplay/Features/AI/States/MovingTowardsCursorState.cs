using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using Assets._Project.Develop.Runtime.Utilities.StateMachineCore;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.AI.States
{
    public class MovingTowardsCursorState : State, IUpdatableState
    {
        private readonly IMouseInputService _mouseInput;
        private readonly ReactiveVariable<Vector3> _moveDirection;
        private readonly ReactiveVariable<Vector3> _rotationDirection;

        private Camera _camera;
        private readonly Transform _transform;

        public MovingTowardsCursorState(
            Entity entity,
            IMouseInputService mouseInput)
        {
            _mouseInput = mouseInput;
            _transform = entity.Transform;
            _moveDirection = entity.MoveDirection;
            _rotationDirection = entity.RotationDirection;
        }

        public override void Enter()
        {
            base.Enter();
            _camera = Camera.main;
        }

        public void Update(float deltaTime)
        {
            Vector3 entityScreenPoint = _camera.WorldToScreenPoint(_transform.position);

            Vector3 mousePointerAtDepth = new Vector3(
                _mouseInput.PointerScreenPosition.x,
                _mouseInput.PointerScreenPosition.y,
                entityScreenPoint.z);

            Vector3 pointerAtWorldPoint = _camera.ScreenToWorldPoint(mousePointerAtDepth);

            Vector3 direction = pointerAtWorldPoint - _transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.05f)
            {
                _moveDirection.Value = Vector3.zero;
                return;
            }
            
            direction.Normalize();

            _moveDirection.Value = direction;
            _rotationDirection.Value = direction;
        }
    }
}