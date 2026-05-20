using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using Assets._Project.Develop.Runtime.Utilities.StateMachineCore;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.AI.States
{
    public class InformativeIdleState : State, IUpdatableState
    {
        private readonly ReactiveVariable<bool> _isCurrentlyIdle;
        private readonly ReactiveVariable<Vector3> _moveDirection;
        private readonly ReactiveVariable<Vector3> _rotationDirection;
        
        public InformativeIdleState(Entity entity)
        {
            _isCurrentlyIdle = entity.IsCurrentlyIdle;
            _moveDirection = entity.MoveDirection;
            _rotationDirection = entity.RotationDirection;
        }
        
        public override void Enter()
        {
            base.Enter();
            
            _moveDirection.Value = Vector3.zero;
            _rotationDirection.Value = Vector3.zero;
            
            _isCurrentlyIdle.Value = true;
        }
        public override void Exit()
        {
            base.Exit();
            
            _isCurrentlyIdle.Value = false;
        }
        
        public void Update(float deltaTime)
        {
        }
    }
}