using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Utilities.Conditions;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.MovementFeature
{
    public class RigidbodyMovementSystem : IInitializableSystem, IUpdatableSystem
    {
        private ReactiveVariable<Vector3> _moveDirection;
        private ReactiveVariable<float> _moveSpeed;
        private Rigidbody _rigidbody;
        private ReactiveVariable<bool> _isMoving;

        private ICompositeCondition _canMove;
        private ReactiveVariable<bool> _hasCollided;

        public void OnInit(Entity entity)
        {
            _moveDirection = entity.MoveDirection;
            _moveSpeed = entity.MoveSpeed;
            _rigidbody = entity.Rigidbody;
            _isMoving = entity.IsMoving;

            _canMove = entity.CanMove;

            if (entity.TryGetHasCollided(out ReactiveVariable<bool> hasCollided) == true)
                _hasCollided = hasCollided;
        }

        public void OnUpdate(float deltaTime)
        {
            if (_rigidbody.isKinematic)
            {
                _isMoving.Value = false;
                return;
            }

            if (_hasCollided != null && _hasCollided.Value == true)
            {
                _isMoving.Value = _rigidbody.velocity.sqrMagnitude > 0f;
                return;
            }

            if (_canMove.Evaluate() == false)
            {
                _rigidbody.velocity = Vector3.zero;
                _isMoving.Value = false;
                return;
            }

            Vector3 velocity = _moveDirection.Value.normalized * _moveSpeed.Value;
            _isMoving.Value = velocity.magnitude > 0;
            _rigidbody.velocity = velocity;
        }
    }
}