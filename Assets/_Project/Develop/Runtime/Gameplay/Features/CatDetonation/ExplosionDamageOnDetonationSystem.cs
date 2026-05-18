using System;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Utilities.Reactive;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.DistanceDetector
{
    public class ExplosionDamageOnDetonationSystem : IInitializableSystem, IDisposableSystem
    {
        private ReactiveVariable<float> _damage;
        private ReactiveVariable<Entity> _target;
        private Entity _source;
        
        private IDisposable _startExplosionDisposable;
        
        public void OnInit(Entity entity)
        {
            _source = entity;
            _damage = entity.ExplosionDamage;
            _target = entity.CurrentTarget;

            _startExplosionDisposable = entity.StartExplosionEvent.Subscribe(OnExplosionStarted);
        }

        private void OnExplosionStarted()
        {
            if (_target.Value == null)
                return;
            
            EntitiesHelper.TryTakeDamageFrom(_source, _target.Value, _damage.Value);
        }

        public void OnDispose()
        {
            _startExplosionDisposable.Dispose();
        }
    }
}