using System;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Utilities.Reactive;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.DealDamageOnTargetReached
{
    public class DealDamageOnTargetReachedSystem : IInitializableSystem, IDisposableSystem
    {
        private IDisposable _targetReachedRequest;
        private ReactiveVariable<float> _explosionDamage;
        private ReactiveVariable<Entity> _target;
        private Entity _entity;
        
        public void OnInit(Entity entity)
        {
            _explosionDamage = entity.ExplosionDamage;
            _target = entity.CurrentTarget;
            _entity = entity;
            _targetReachedRequest = entity.DistanceToTargetReachedEvent.Subscribe(OnTargetReached);
        }

        private void OnTargetReached()
        {
            if (_target.Value == null)
                return;
            
            EntitiesHelper.TryTakeDamageFrom(_entity, _target.Value, _explosionDamage.Value);
        }

        public void OnDispose()
        {
            _targetReachedRequest.Dispose();
        }
    }
}