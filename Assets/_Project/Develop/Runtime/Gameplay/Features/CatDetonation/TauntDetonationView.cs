using System;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using Assets._Project.Develop.Runtime.Utilities.Audio;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using CartoonFXPack;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.DistanceDetector
{
[RequireComponent(typeof(Animator))]
    public class TauntDetonationView : EntityView
    {
        private static readonly int TauntIndexKey = Animator.StringToHash("TauntIndex");
        private static readonly int DetonateKey = Animator.StringToHash("Detonate");

        [SerializeField] private Animator _animator;
        
        [SerializeField] private Transform _explosionEffectPoint;
        [SerializeField] private Particle _explosionEffectPrefab;
        
        [SerializeField] private GameSoundsIDs _tauntSound = GameSoundsIDs.CatTaunt;
        [SerializeField] private GameSoundsIDs _explosionSound = GameSoundsIDs.Explosion;
        [SerializeField] private AudioSource _localAudioSource;
        
        private ReactiveEvent _startTauntEvent;
        private ReactiveVariable<int> _detonateTauntIndex;
        private ReactiveEvent _startExplosionEvent;
        private ReactiveEvent _hideExplosionSourceEvent;
        
        private IDisposable _startTauntDisposable;
        private IDisposable _startExplosionDisposable;
        private IDisposable _hideSourceDisposable;
        
        private void OnValidate()
        {
            _animator ??= GetComponent<Animator>();
        }

        protected override void OnEntityStartedWork(Entity entity)
        {
            _startTauntEvent = entity.StartTauntEvent;
            _detonateTauntIndex = entity.DetonateTauntIndex;
            _startExplosionEvent = entity.StartExplosionEvent;
            _hideExplosionSourceEvent = entity.HideExplosionSourceEvent;
            
            _startTauntDisposable = _startTauntEvent.Subscribe(OnStartTaunt);
            _startExplosionDisposable = _startExplosionEvent.Subscribe(OnStartExplosion);
            _hideSourceDisposable = _hideExplosionSourceEvent.Subscribe(OnHideExplosionSource);
        }

        private void OnStartTaunt()
        {
            _animator.SetInteger(TauntIndexKey, _detonateTauntIndex.Value);
            _animator.SetTrigger(DetonateKey);

            if (_localAudioSource == null)
                return;

            GameSoundsService.PlayOneShot(_tauntSound, _localAudioSource);
        }

        private void OnStartExplosion()
        {
            if (_explosionEffectPrefab == null || _explosionEffectPoint == null)
                return;

            Instantiate(
                _explosionEffectPrefab,
                _explosionEffectPoint.position,
                _explosionEffectPrefab.transform.rotation);

            if (_localAudioSource == null)
                return;

            GameSoundsService.PlayOneShot(_explosionSound, _localAudioSource);
        }

        private void OnHideExplosionSource()
        {
            gameObject.SetActive(false);
        }


        public override void Cleanup(Entity entity)
        {
            base.Cleanup(entity);

            _startTauntDisposable?.Dispose();
            _startExplosionDisposable?.Dispose();
            _hideSourceDisposable?.Dispose();
        }
    }
}