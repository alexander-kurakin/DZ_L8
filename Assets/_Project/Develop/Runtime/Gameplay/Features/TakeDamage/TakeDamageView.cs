using System;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.TakeDamage
{
    public class TakeDamageView : EntityView
    {
        [SerializeField] private Transform _effectSpawnPoint;
        [SerializeField] private ParticleSystem _takeDamageEffectPrefab;

        private ReactiveEvent<float> _damageEvent;
        private IDisposable _damageEventDisposable;
        
        protected override void OnEntityStartedWork(Entity entity)
        {
            _damageEvent = entity.TakeDamageEvent;
            _damageEventDisposable = _damageEvent.Subscribe(OnDamageTaken);
        }

        private void OnDamageTaken(float obj)
        {
            Instantiate(_takeDamageEffectPrefab, _effectSpawnPoint.position, Quaternion.identity);
        }

        public override void Cleanup(Entity entity)
        {
            base.Cleanup(entity);
            
            _damageEventDisposable?.Dispose();
        }
    }
}