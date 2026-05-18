using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System;
using Assets._Project.Develop.Runtime.Utilities.Audio;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.SpawnFeature
{
    [RequireComponent(typeof(Animator))]
    public class SpawnProcessView : EntityView
    {
        private readonly int SpawningProcessKey = Animator.StringToHash("InSpawnProcess");
        private const float VolumeMinMult = 0.9f, VolumeMaxMult = 1.2f, PitchMin = 0.7f, PitchMax = 1.2f;

        [SerializeField] private Animator _animator;
        [SerializeField] private ParticleSystem _spawnEffectPrefab;
        [SerializeField] private Transform _spawnEffectPoint;
        
        [SerializeField] private GameSoundsIDs _spawnSoundToPlay;
        [SerializeField] private AudioSource _localAudioSource;
        
        private ReactiveVariable<bool> _inSpawnProcess;
        private ParticleSystem _spawnGlowInstance;

        private IDisposable _inSpawnProcessChangedDisposable;

        private void OnValidate()
        {
            _animator ??= GetComponent<Animator>();
        }

        protected override void OnEntityStartedWork(Entity entity)
        {
            _inSpawnProcess = entity.InSpawnProcess;

            _inSpawnProcessChangedDisposable = _inSpawnProcess.Subscribe(OnSpawnProcessChanged);
            UpdateSpawnProcessKey(_inSpawnProcess.Value);
            
            if (_inSpawnProcess.Value)
                PlayEffects();
        }

        public override void Cleanup(Entity entity)
        {
            base.Cleanup(entity);

            _inSpawnProcessChangedDisposable.Dispose();
        }

        private void OnSpawnProcessChanged(bool oldValue, bool newValue)
        {
            UpdateSpawnProcessKey(newValue);

            if (newValue && oldValue == false)
                PlayEffects();
        }

        private void PlayEffects()
        {
            if (_spawnEffectPrefab == null || _spawnEffectPoint == null)
                return;
            
            Instantiate(
                _spawnEffectPrefab,
                _spawnEffectPoint.position,
                _spawnEffectPrefab.transform.rotation,
                null);

            if (_localAudioSource == null)
                return;
                
            SetupRandomSettingsOnAudioSource();
            GameSoundsService.PlayOneShot(_spawnSoundToPlay, _localAudioSource);
        }

        private void SetupRandomSettingsOnAudioSource()
        {
            _localAudioSource.volume = 
                Random.Range(_localAudioSource.volume * VolumeMinMult, 
                             _localAudioSource.volume * VolumeMaxMult);
            _localAudioSource.pitch = Random.Range(PitchMin, PitchMax);
        }

        private void UpdateSpawnProcessKey(bool value)
        {
            _animator.SetBool(SpawningProcessKey, value);
        }
    }
}
