using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.TowerWalker;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using Assets._Project.Develop.Runtime.Utilities.StateMachineCore;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.AI.States
{
    public class RandomMovementState : State, IUpdatableState
    {
        private const int DIRECTION_PICK_ATTEMPTS = 8;

        private readonly ReactiveVariable<Vector3> _movementDirection;
        private readonly ReactiveVariable<Vector3> _rotationDirection;
        private readonly bool _respectTowerWalkBounds;
        private readonly CapsuleCollider _bodyCollider;

        private Vector3 _lastMovementDirection;

        public RandomMovementState(Entity entity, bool respectTowerWalkBounds = false)
        {
            _movementDirection = entity.MoveDirection;
            _rotationDirection = entity.RotationDirection;
            _respectTowerWalkBounds = respectTowerWalkBounds;
            _bodyCollider = null;

            if (_respectTowerWalkBounds)
                entity.TryGetBodyCollider(out _bodyCollider);
        }

        public override void Enter()
        {
            base.Enter();

            Vector3 direction;

            if (_lastMovementDirection.sqrMagnitude > 0.05f)
                direction = TryPickUnblockedDirection(GenerateNewInverseTurnDirection(_lastMovementDirection));
            else
                direction = TryPickUnblockedDirection(GenerateNewRandomDirection());

            SetDirection(direction);
            _lastMovementDirection = direction;
        }

        public override void Exit()
        {
            base.Exit();

            _movementDirection.Value = Vector3.zero;
            _rotationDirection.Value = Vector3.zero;
        }

        public void Update(float deltaTime)
        {
            if (_respectTowerWalkBounds == false)
                return;

            if (_movementDirection.Value.sqrMagnitude <= 0.0001f)
                return;

            if (TowerWalkBoundsMovementUtility.IsMovementBlockedByTowerEdge(
                    _bodyCollider,
                    _movementDirection.Value) == false)
                return;

            Vector3 direction = TryPickUnblockedDirection(GenerateNewRandomDirection());
            SetDirection(direction);
            _lastMovementDirection = direction;
        }

        private Vector3 TryPickUnblockedDirection(Vector3 preferredDirection)
        {
            if (_respectTowerWalkBounds == false)
                return preferredDirection;

            if (preferredDirection.sqrMagnitude > 0.0001f
                && TowerWalkBoundsMovementUtility.IsMovementBlockedByTowerEdge(
                    _bodyCollider,
                    preferredDirection) == false)
                return preferredDirection;

            for (int attempt = 0; attempt < DIRECTION_PICK_ATTEMPTS; attempt++)
            {
                Vector3 candidate = GenerateNewRandomDirection();

                if (TowerWalkBoundsMovementUtility.IsMovementBlockedByTowerEdge(_bodyCollider, candidate) == false)
                    return candidate;
            }

            return Vector3.zero;
        }

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
