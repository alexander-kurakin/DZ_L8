using System;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Utilities.Reactive;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.DealDamageOnTargetReached
{
    public class DealOneOffDamageOnTargetReachedSystem : IInitializableSystem, IDisposableSystem
    {
        private IDisposable _targetReachedRequest;
        private ReactiveVariable<float> _oneOffDamage;
        private ReactiveVariable<Entity> _target;
        private Entity _source;
        
        public void OnInit(Entity entity)
        {
            _source = entity;
            
            _oneOffDamage = entity.ExplosionDamage;
            _target = entity.CurrentTarget;
            
            _targetReachedRequest = entity.DistanceToTargetReachedEvent.Subscribe(OnTargetReached);
        }

        private void OnTargetReached()
        {
            if (_target.Value == null)
                return;
            
            EntitiesHelper.TryTakeDamageFrom(_source, _target.Value, _oneOffDamage.Value);
        }

        public void OnDispose()
        {
            _targetReachedRequest.Dispose();
        }
    }
}