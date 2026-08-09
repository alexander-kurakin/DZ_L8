using System;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using Assets._Project.Develop.Runtime.Gameplay.Features.JuiceFeature;
using Assets._Project.Develop.Runtime.Utilities.Audio;
using DG.Tweening;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.EssenceFeature
{
    public class EssencePickupView : EntityView
    {
        [SerializeField] private GameObject _vacuumTrailPrefab;
        [SerializeField] private GameObject _towerCollectPrefab;
        [SerializeField] private GameSoundsIDs _pickupSoundToPlay = GameSoundsIDs.PickupCrystal;

        [SerializeField] private float _towerCollectVfxScale = 6f;
        [SerializeField] private float _groundScale = 2.88f;
        [SerializeField] private float _hoverReadyScaleFactor = 1.35f;
        [SerializeField] private float _hoverReadyGrowDurationSeconds = 0.5f;
        [SerializeField] private float _vacuumPulseScaleFactor = 1.6f;
        [SerializeField] private float _vacuumPulseUpDurationSeconds = 0.22f;
        [SerializeField] private float _vacuumSettleDurationSeconds = 0.28f;
        [SerializeField] private float _trailLocalY = 9f;
        [SerializeField] private float _trailScale = 2f;

        private Transform _visualRoot;
        private GameObject _vacuumTrailInstance;
        private Tween _vacuumScaleTween;
        private Tween _hoverPopScaleTween;
        private IDisposable _hoverReadyDisposable;
        private IDisposable _vacuumStartedDisposable;
        private IDisposable _collectedDisposable;

        protected override void OnEntityStartedWork(Entity entity)
        {
            _visualRoot = transform.childCount > 0 ? transform.GetChild(0) : transform;
            _visualRoot.localScale = Vector3.one * _groundScale;

            _hoverReadyDisposable = entity.EssenceHoverReadyEvent.Subscribe(OnHoverReady);
            _vacuumStartedDisposable = entity.EssenceVacuumStartedEvent.Subscribe(OnVacuumStarted);
            _collectedDisposable = entity.EssenceCollectedEvent.Subscribe(OnCollected);
        }

        public override void Cleanup(Entity entity)
        {
            base.Cleanup(entity);

            _hoverReadyDisposable?.Dispose();
            _vacuumStartedDisposable?.Dispose();
            _collectedDisposable?.Dispose();

            KillTweens();
        }

        private void OnHoverReady()
        {
            _hoverPopScaleTween?.Kill();
            _hoverPopScaleTween = _visualRoot
                .DOScale(Vector3.one * GetHoverScale(), _hoverReadyGrowDurationSeconds)
                .SetEase(Ease.OutCubic)
                .Play();
        }

        private void OnVacuumStarted()
        {
            AttachVacuumTrail();

            _hoverPopScaleTween?.Kill();
            _vacuumScaleTween?.Kill();

            float hoverScale = GetHoverScale();
            float pulseScale = GetVacuumPulseScale();
            _vacuumScaleTween = DOTween.Sequence()
                .Append(_visualRoot
                    .DOScale(Vector3.one * pulseScale, _vacuumPulseUpDurationSeconds)
                    .SetEase(Ease.OutBack, 1.4f, 0.35f))
                .Append(_visualRoot
                    .DOScale(Vector3.one * hoverScale, _vacuumSettleDurationSeconds)
                    .SetEase(Ease.OutCubic))
                .Play();
        }

        private void OnCollected()
        {
            KillTweens();
            GameSoundsService.PlayOneShot(_pickupSoundToPlay);
            PlayTowerCollectVfx();
        }

        private void PlayTowerCollectVfx()
        {
            if (_towerCollectPrefab == null)
                return;

            GameplayVfxUtility.SpawnTransientAt(
                _towerCollectPrefab,
                transform.position,
                Quaternion.identity,
                null,
                _towerCollectVfxScale);
        }

        private void AttachVacuumTrail()
        {
            if (_vacuumTrailPrefab == null)
                return;

            if (_vacuumTrailInstance != null)
                return;

            _vacuumTrailInstance = Instantiate(_vacuumTrailPrefab, transform);
            _vacuumTrailInstance.transform.localPosition = new Vector3(0f, _trailLocalY, 0f);
            _vacuumTrailInstance.transform.localRotation = Quaternion.identity;
            _vacuumTrailInstance.transform.localScale = Vector3.one * _trailScale;

            ParticleSystem[] particleSystems = _vacuumTrailInstance.GetComponentsInChildren<ParticleSystem>(true);

            for (int index = 0; index < particleSystems.Length; index++)
                particleSystems[index].Play(true);
        }

        private float GetHoverScale()
        {
            return _groundScale * _hoverReadyScaleFactor;
        }

        private float GetVacuumPulseScale()
        {
            return _groundScale * _vacuumPulseScaleFactor;
        }

        private void KillTweens()
        {
            _vacuumScaleTween?.Kill();
            _hoverPopScaleTween?.Kill();
            _vacuumScaleTween = null;
            _hoverPopScaleTween = null;
        }

        private void OnDestroy()
        {
            KillTweens();
        }
    }
}
