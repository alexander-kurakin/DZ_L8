using System;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.DistanceDetector
{
    public class DistanceDetectorSystem : IInitializableSystem, IUpdatableSystem
    {
        private Transform _currentTransform;
        private Transform _currentTargetTransform;
        
        private ReactiveVariable<float> _distanceToTargetGoal;
        private ReactiveVariable<float> _distanceToTargetCurrent;
        private ReactiveVariable<Entity> _currentTarget;

        private ReactiveVariable<bool> _targetDistanceReached;
        private ReactiveEvent _targetDistanceReachedEvent;
        

        public void OnInit(Entity entity)
        {
            _currentTransform = entity.Transform;
            _currentTarget = entity.CurrentTarget;
            
            _distanceToTargetGoal = entity.DistanceToTargetGoal;
            _distanceToTargetCurrent = entity.DistanceToTargetCurrent;
            _targetDistanceReachedEvent = entity.DistanceToTargetReachedEvent;
            _targetDistanceReached = entity.DistanceToTargetReached;

        }

        public void OnUpdate(float deltaTime)
        {
            if (_targetDistanceReached.Value || _currentTarget.Value == null)
                return;
            
            _currentTargetTransform = _currentTarget.Value.Transform;
            
            _distanceToTargetCurrent.Value = (_currentTargetTransform.position - _currentTransform.position).magnitude;

            if (_distanceToTargetCurrent.Value <= _distanceToTargetGoal.Value)
            {
                Debug.Log("Я дошел до цели!");
                _targetDistanceReached.Value = true;
                _targetDistanceReachedEvent?.Invoke();
            }
        }
    }
}