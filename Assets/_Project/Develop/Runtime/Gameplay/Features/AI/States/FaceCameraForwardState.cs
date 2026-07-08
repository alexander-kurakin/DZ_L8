using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using Assets._Project.Develop.Runtime.Utilities.StateMachineCore;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.AI.States
{
    public class FaceCameraForwardState : State, IUpdatableState
    {
        private readonly ReactiveVariable<Vector3> _movementDirection;
        private readonly ReactiveVariable<Vector3> _rotationDirection;

        public FaceCameraForwardState(Entity entity)
        {
            _movementDirection = entity.MoveDirection;
            _rotationDirection = entity.RotationDirection;
        }

        public override void Enter()
        {
            base.Enter();

            _movementDirection.Value = Vector3.zero;
            ApplyCameraForwardRotation();
        }

        public void Update(float deltaTime)
        {
            ApplyCameraForwardRotation();
        }

        private void ApplyCameraForwardRotation()
        {
            Camera camera = Camera.main;

            if (camera == null)
                return;

            Vector3 cameraForward = camera.transform.forward;
            cameraForward.y = 0f;

            if (cameraForward.sqrMagnitude <= 0f)
                return;

            _rotationDirection.Value = cameraForward.normalized;
        }
    }
}
