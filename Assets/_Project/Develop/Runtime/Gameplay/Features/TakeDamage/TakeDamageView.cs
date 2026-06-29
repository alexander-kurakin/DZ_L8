using System;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using Assets._Project.Develop.Runtime.Gameplay.Features.JuiceFeature;
using Assets._Project.Develop.Runtime.Utilities.Audio;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using DamageNumbersPro;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.TakeDamage
{
    public class TakeDamageView : EntityView
    {
        [SerializeField] private Transform _effectSpawnPoint;
        [SerializeField] private GameObject _defaultDamageEffectPrefab;
        [SerializeField] private GameObject _sectorAbilityDamageEffectPrefab;
        [SerializeField] private GameObject _mineDamageEffectPrefab;
        [SerializeField] private float _mineDamageEffectScale = 10f;
        [SerializeField] private Vector3 _mineDamageEffectScaleMultiplier = new Vector3(2f, 1f, 2f);
        [SerializeField] private DamageNumber _damageNumberPrefab;

        [SerializeField] private GameSoundsIDs _impactSoundToPlay;
        [SerializeField] private AudioSource _localAudioSource;

        private ReactiveEvent<TakeDamageInfo> _damageEvent;
        private IDisposable _damageEventDisposable;

        protected override void OnEntityStartedWork(Entity entity)
        {
            _damageEvent = entity.TakeDamageEvent;
            _damageEventDisposable = _damageEvent.Subscribe(OnDamageTaken);
            _damageNumberPrefab.SetScale(7f);
        }

        private void OnDamageTaken(TakeDamageInfo damageInfo)
        {
            GameObject effectPrefab = ResolveEffectPrefab(damageInfo.VisualKind);

            if (effectPrefab != null)
            {
                SpawnDamageEffect(effectPrefab, damageInfo.VisualKind);
            }

            _damageNumberPrefab.Spawn(_effectSpawnPoint.position, damageInfo.Damage);
            DamageSilhouetteFlashUtility.PlayOnTransform(transform, damageInfo.Damage);
            GameSoundsService.PlayOneShot(_impactSoundToPlay, _localAudioSource);
        }

        private GameObject ResolveEffectPrefab(TakeDamageVisualKind visualKind)
        {
            switch (visualKind)
            {
                case TakeDamageVisualKind.SectorAbility:
                    return _sectorAbilityDamageEffectPrefab != null
                        ? _sectorAbilityDamageEffectPrefab
                        : _defaultDamageEffectPrefab;

                case TakeDamageVisualKind.Mine:
                    return _mineDamageEffectPrefab != null
                        ? _mineDamageEffectPrefab
                        : _defaultDamageEffectPrefab;

                default:
                    return _defaultDamageEffectPrefab;
            }
        }

        private void SpawnDamageEffect(GameObject effectPrefab, TakeDamageVisualKind visualKind)
        {
            if (visualKind == TakeDamageVisualKind.Mine)
            {
                GameObject instance = GameplayVfxUtility.SpawnAt(
                    effectPrefab,
                    _effectSpawnPoint.position,
                    Quaternion.identity,
                    null,
                    1f);

                if (instance == null)
                    return;

                instance.transform.localScale = new Vector3(
                    _mineDamageEffectScale * _mineDamageEffectScaleMultiplier.x,
                    _mineDamageEffectScale * _mineDamageEffectScaleMultiplier.y,
                    _mineDamageEffectScale * _mineDamageEffectScaleMultiplier.z);
                GameplayVfxUtility.ScheduleDestroyAfterLifetime(instance);
                return;
            }

            GameplayVfxUtility.SpawnTransientAt(
                effectPrefab,
                _effectSpawnPoint.position,
                Quaternion.identity,
                null,
                1f);
        }

        public override void Cleanup(Entity entity)
        {
            base.Cleanup(entity);

            _damageEventDisposable?.Dispose();
        }
    }
}
