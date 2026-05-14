using System;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Utilities.Reactive;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.DealDamageOnTargetReached
{
    public class DealDoTDamageOnTargetReachedSystem : IInitializableSystem, IDisposableSystem, IUpdatableSystem
    {
        private IDisposable _targetReachedRequest;
        private ReactiveVariable<Entity> _target;
        private Entity _source;
        
        private ReactiveVariable<float> _damagePerTick;
        private ReactiveVariable<float> _dotTickInterval;
        private ReactiveVariable<float> _dotTimer;
        
        private bool _targetReached;
        
        public void OnInit(Entity entity)
        {
            _target = entity.CurrentTarget;
            _source = entity;
            
            _damagePerTick = entity.DamagePerTick;
            _dotTickInterval = entity.DamageInterval;
            _dotTimer = entity.DamageTimer;

            _targetReachedRequest = entity.DistanceToTargetReachedEvent.Subscribe(OnTargetReached);
        }

        private void OnTargetReached()
        {
            if (_target.Value == null)
                return;

            _targetReached = true;
        }
        
        public void OnDispose()
        {
            _targetReachedRequest.Dispose();
        }

        public void OnUpdate(float deltaTime)
        {
            if (_targetReached)
            {
                _dotTimer.Value -= deltaTime;

                if (_dotTimer.Value > 0f)
                    return;

                EntitiesHelper.TryTakeDamageFrom(_source, _target.Value, _damagePerTick.Value);
            
                _dotTimer.Value = _dotTickInterval.Value;
            }
        }
    }
}