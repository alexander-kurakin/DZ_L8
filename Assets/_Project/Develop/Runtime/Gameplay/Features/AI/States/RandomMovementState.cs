using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using Assets._Project.Develop.Runtime.Utilities.StateMachineCore;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.AI.States
{
    public class RandomMovementState : State, IUpdatableState
    {
        private ReactiveVariable<Vector3> _movementDirection;
        private ReactiveVariable<Vector3> _rotationDirection;
        private Vector3 _lastMovementDirection;

        public RandomMovementState(Entity entity)
        {
            _movementDirection = entity.MoveDirection;
            _rotationDirection = entity.RotationDirection;
        }

        public override void Enter()
        {
            base.Enter();

            if (_lastMovementDirection.sqrMagnitude > 0.05f)
                SetDirection(GenerateNewInverseTurnDirection(_lastMovementDirection));
            else
                SetDirection(GenerateNewRandomDirection());
            
            _lastMovementDirection = _movementDirection.Value;
        }

        public override void Exit()
        {
            base.Exit();

            _movementDirection.Value = Vector3.zero;
            _rotationDirection.Value = Vector3.zero;
        }

        public void Update(float deltaTime) { }

        private Vector3 GenerateNewRandomDirection()
        {
            return new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
        }

        private Vector3 GenerateNewInverseTurnDirection(Vector3 previousDirection)
        {
            Vector3 inverseDirection = -previousDirection.normalized;
            Quaternion randomTurn = Quaternion.Euler(0, Random.Range(-30, 30), 0);
            
            return randomTurn * inverseDirection;
        }
        
        private void SetDirection(Vector3 direction)
        {
            _movementDirection.Value = direction;
            _rotationDirection.Value = direction;
        }
    }
}
