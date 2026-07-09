using System;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using Assets._Project.Develop.Runtime.Utilities.Audio;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.TakeDamage
{
    public class TakeDamageView : EntityView
    {
        [SerializeField] private Transform _effectSpawnPoint;
        [SerializeField] private GameObject _defaultDamageEffectPrefab;
        [SerializeField] private GameSoundsIDs _impactSoundToPlay;
        [SerializeField] private AudioSource _localAudioSource;

        private ReactiveEvent<TakeDamageInfo> _damageEvent;
        private IDisposable _damageEventDisposable;
        private Entity _entity;

        protected override void OnEntityStartedWork(Entity entity)
        {
            _entity = entity;
            _damageEvent = entity.TakeDamageEvent;
            _damageEventDisposable = _damageEvent.Subscribe(OnDamageTaken);
        }

        private void OnDamageTaken(TakeDamageInfo damageInfo)
        {
            if (_defaultDamageEffectPrefab != null)
                SpawnDamageEffect(_defaultDamageEffectPrefab);

            DamageSilhouetteFlashUtility.PlayOnTransform(transform, damageInfo.Damage);

            if (_entity.TryGetEnemySpawnOrigin(out Vector3 spawnOrigin))
                EnemyHitJuiceUtility.PlayOnTransform(transform, spawnOrigin, damageInfo.Damage);

            GameSoundsService.PlayOneShot(_impactSoundToPlay, _localAudioSource);
        }

        private void SpawnDamageEffect(GameObject effectPrefab)
        {
            GameObject instance = Instantiate(effectPrefab, _effectSpawnPoint.position, Quaternion.identity);

            if (instance == null)
                return;

            Destroy(instance, 2f);
        }

        public override void Cleanup(Entity entity)
        {
            base.Cleanup(entity);

            _damageEventDisposable?.Dispose();
        }
    }
}
