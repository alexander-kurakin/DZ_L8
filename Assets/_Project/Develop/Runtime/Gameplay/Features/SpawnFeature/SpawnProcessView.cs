using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.SpawnFeature
{
    [RequireComponent(typeof(Animator))]
    public class SpawnProcessView : EntityView
    {
        private readonly int SpawningProcessKey = Animator.StringToHash("InSpawnProcess");
        private const float VolumeMin = 0.6f, VolumeMax = 1f, PitchMin = 0.7f, PitchMax = 1.2f;

        [SerializeField] private Animator _animator;
        [SerializeField] private ParticleSystem _spawnEffectPrefab;
        [SerializeField] private Transform _spawnEffectPoint;
        [SerializeField] private AudioClip _spawnVfxSound;
        [SerializeField] private AudioSource _audioSource;
        
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
            OnSpawnProcessChanged(default, _inSpawnProcess.Value);
        }

        public override void Cleanup(Entity entity)
        {
            base.Cleanup(entity);

            _inSpawnProcessChangedDisposable.Dispose();
        }

        private void OnSpawnProcessChanged(bool arg1, bool newValue)
        {
            UpdateSpawnProcessKey(newValue);
            
            if (newValue)
            {
                Instantiate(
                    _spawnEffectPrefab,
                    _spawnEffectPoint.position,
                    _spawnEffectPrefab.transform.rotation,
                    null);
                
                SetupRandomSettingsOnAudioSource();
                _audioSource.PlayOneShot(_spawnVfxSound);
            }
        }

        private void SetupRandomSettingsOnAudioSource()
        {
            _audioSource.volume = Random.Range(VolumeMin, VolumeMax);
            _audioSource.pitch = Random.Range(PitchMin, PitchMax);
        }

        private void UpdateSpawnProcessKey(bool value)
        {
            _animator.SetBool(SpawningProcessKey, value);
        }
    }
}
