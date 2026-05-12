using System;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using Assets._Project.Develop.Runtime.Utilities.Audio;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using DamageNumbersPro;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.TakeDamage
{
    public class TakeDamageView : EntityView
    {
        [SerializeField] private Transform _effectSpawnPoint;
        [SerializeField] private ParticleSystem _takeDamageEffectPrefab;
        [SerializeField] private DamageNumber _damageNumberPrefab;
        
        [SerializeField] private GameSoundsIDs _impactSoundToPlay;
        [SerializeField] private AudioSource _localAudioSource;

        private ReactiveEvent<float> _damageEvent;
        private IDisposable _damageEventDisposable;
        
        protected override void OnEntityStartedWork(Entity entity)
        {
            _damageEvent = entity.TakeDamageEvent;
            _damageEventDisposable = _damageEvent.Subscribe(OnDamageTaken);
            _damageNumberPrefab.SetScale(7f);
        }

        private void OnDamageTaken(float damage)
        {
            Instantiate(_takeDamageEffectPrefab, _effectSpawnPoint.position, Quaternion.identity);
            _damageNumberPrefab.Spawn(_effectSpawnPoint.position, damage);
            GameSoundsService.PlayOneShot(_impactSoundToPlay, _localAudioSource);
        }

        public override void Cleanup(Entity entity)
        {
            base.Cleanup(entity);
            
            _damageEventDisposable?.Dispose();
        }
    }
}