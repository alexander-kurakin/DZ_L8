using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using Assets._Project.Develop.Runtime.Utilities.StateMachineCore;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.AI.States
{
    public class MoveToTargetState : State, IUpdatableState
    {
        private ReactiveVariable<Vector3> _moveDirection;
        private ReactiveVariable<Entity> _currentTarget;
        private Transform _transform;

        public MoveToTargetState(Entity entity)
        {
            _moveDirection = entity.MoveDirection;
            _currentTarget = entity.CurrentTarget;
            _transform = entity.Transform;
        }

        public void Update(float deltaTime)
        {
            if (_currentTarget.Value != null)
                _moveDirection.Value = (_currentTarget.Value.Transform.position - _transform.position).normalized;
            else
                _moveDirection.Value = Vector3.zero;
        }
    }
}