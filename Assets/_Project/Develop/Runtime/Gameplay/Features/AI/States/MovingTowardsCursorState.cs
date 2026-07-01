using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.TowerWalker;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using Assets._Project.Develop.Runtime.Utilities.StateMachineCore;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.AI.States
{
    public class MovingTowardsCursorState : State, IUpdatableState
    {
        private const float MIN_DIRECTION_SQR_MAGNITUDE = 0.0025f;

        private readonly IMouseInputService _mouseInput;
        private readonly ReactiveVariable<Vector3> _moveDirection;
        private readonly ReactiveVariable<Vector3> _rotationDirection;
        private readonly Transform _transform;
        private readonly bool _respectTowerWalkBounds;
        private readonly CapsuleCollider _bodyCollider;

        private Camera _camera;

        public MovingTowardsCursorState(
            Entity entity,
            IMouseInputService mouseInput,
            bool respectTowerWalkBounds = false)
        {
            _mouseInput = mouseInput;
            _transform = entity.Transform;
            _moveDirection = entity.MoveDirection;
            _rotationDirection = entity.RotationDirection;
            _respectTowerWalkBounds = respectTowerWalkBounds;
            _bodyCollider = null;

            if (_respectTowerWalkBounds)
                entity.TryGetBodyCollider(out _bodyCollider);
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

            Vector3 directionToCursor = pointerAtWorldPoint - _transform.position;
            directionToCursor.y = 0f;

            if (directionToCursor.sqrMagnitude < MIN_DIRECTION_SQR_MAGNITUDE)
            {
                _moveDirection.Value = Vector3.zero;
                return;
            }

            directionToCursor.Normalize();
            _rotationDirection.Value = directionToCursor;

            if (_respectTowerWalkBounds
                && TowerWalkBoundsMovementUtility.IsMovementBlockedByTowerEdge(_bodyCollider, directionToCursor))
            {
                _moveDirection.Value = Vector3.zero;
                return;
            }

            _moveDirection.Value = directionToCursor;
        }
    }
}
