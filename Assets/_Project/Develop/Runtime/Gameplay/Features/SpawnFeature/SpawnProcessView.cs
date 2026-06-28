using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using Assets._Project.Develop.Runtime.Utilities.Audio;
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
        private const float VolumeMinMult = 0.9f;
        private const float VolumeMaxMult = 1.2f;
        private const float PitchMin = 0.7f;
        private const float PitchMax = 1.2f;
        private const float DEFAULT_VFX_LIFETIME_SECONDS = 3f;

        [SerializeField] private Animator _animator;
        [SerializeField] private GameObject _spawnEffectPrefab;
        [SerializeField] private Transform _spawnEffectPoint;

        [SerializeField] private GameSoundsIDs _spawnSoundToPlay;
        [SerializeField] private AudioSource _localAudioSource;

        private ReactiveVariable<bool> _inSpawnProcess;
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

            GameObject spawnEffectInstance = Instantiate(
                _spawnEffectPrefab,
                _spawnEffectPoint.position,
                _spawnEffectPrefab.transform.rotation);

            PlayAllParticleSystems(spawnEffectInstance);
            Destroy(spawnEffectInstance, GetEffectLifetimeSeconds(spawnEffectInstance));

            if (_localAudioSource == null)
                return;

            SetupRandomSettingsOnAudioSource();
            GameSoundsService.PlayOneShot(_spawnSoundToPlay, _localAudioSource);
        }

        private static void PlayAllParticleSystems(GameObject root)
        {
            ParticleSystem[] particleSystems = root.GetComponentsInChildren<ParticleSystem>(true);

            for (int index = 0; index < particleSystems.Length; index++)
                particleSystems[index].Play(true);
        }

        private static float GetEffectLifetimeSeconds(GameObject root)
        {
            float maxLifetimeSeconds = DEFAULT_VFX_LIFETIME_SECONDS;
            ParticleSystem[] particleSystems = root.GetComponentsInChildren<ParticleSystem>(true);

            for (int index = 0; index < particleSystems.Length; index++)
            {
                ParticleSystem particleSystem = particleSystems[index];
                ParticleSystem.MainModule mainModule = particleSystem.main;
                float startLifetime = mainModule.startLifetime.constantMax;

                if (mainModule.startLifetime.mode == ParticleSystemCurveMode.TwoConstants)
                    startLifetime = mainModule.startLifetime.constantMax;

                float effectLifetimeSeconds = mainModule.duration + startLifetime;

                if (effectLifetimeSeconds > maxLifetimeSeconds)
                    maxLifetimeSeconds = effectLifetimeSeconds;
            }

            return maxLifetimeSeconds + 0.35f;
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
