using System;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using Assets._Project.Develop.Runtime.Gameplay.Features.Ability;
using Assets._Project.Develop.Runtime.Utilities.Audio;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.ExplosionAbilityPreview
{
    public class ExplosionAbilityView : EntityView
    {
        private const float GroundYOffset = 0.25f;
        private const float ReferenceRadius = 5f;
        private const float PaddingMult = 2f;
        private const float VFXscalingMult = 0.85f;
        

        [SerializeField] private Transform _indicatorPrefab;
        [SerializeField] private ParticleSystem _castVfxPrefab;
        [SerializeField] private GameSoundsIDs _castSound;
        [SerializeField] private AudioSource _localAudioSource;

        private Transform _indicator;
        private ParticleSystem _castVfxPrefabInstance;

        private ReactiveVariable<Vector3> _previewWorldPoint;
        private ReactiveVariable<bool> _previewVisible;
        private ReactiveVariable<float> _explosionRadius;
        private Entity _explodeAbility;

        private IDisposable _visibleDisposable;
        private IDisposable _pointDisposable;
        private IDisposable _radiusDisposable;
        private IDisposable _explosionRequestedDisposable;

        protected override void OnEntityStartedWork(Entity entity)
        {
            _previewWorldPoint = entity.ExplosionPreviewWorldPoint;
            _previewVisible = entity.ExplosionPreviewVisible;
            
            if (entity.AbilityUserAllAbilities.TryGetValue(AbilityType.ExplodeAtPoint, out _explodeAbility))
            {
                if (_explodeAbility.TryGetAreaImpactRadius(out _explosionRadius))
                {
                    InitIndicator();
                    ApplyRadiusScale();

                    _visibleDisposable = _previewVisible.Subscribe(OnPreviewVisibleChanged);
                    _pointDisposable = _previewWorldPoint.Subscribe(OnPreviewWorldPointChanged);
                    _radiusDisposable = _explosionRadius.Subscribe(OnExplosionRadiusChanged);
                    _explosionRequestedDisposable = _explodeAbility.DealAreaImpactDamageRequest.Subscribe(OnExplosionRequested);

                    OneOffSyncFromState();
                }
            }
        }

        private float GetVFXScale(float explosionRadius)
        {
            return VFXscalingMult * (explosionRadius / ReferenceRadius);
        }

        private void OnExplosionRequested(Vector3 worldPoint)
        {
            _castVfxPrefabInstance = Instantiate(_castVfxPrefab, worldPoint, Quaternion.identity);
            
            float scale = GetVFXScale(_explosionRadius.Value);
            _castVfxPrefabInstance.transform.localScale = new Vector3(scale, 1f, scale);
            
            GameSoundsService.PlayOneShot(_castSound, _localAudioSource);
        }

        private void OnDrawGizmos()
        {
            if (_indicator == null || _explosionRadius == null || _previewVisible.Value == false)
                return;
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_previewWorldPoint.Value, _explosionRadius.Value);
        }

        private void InitIndicator()
        {
            _indicator = Instantiate(_indicatorPrefab);
            _indicator.gameObject.SetActive(false);
        }

        private void OnPreviewVisibleChanged(bool oldVisible, bool newVisible)
        {
            _indicator.gameObject.SetActive(newVisible);
        }

        private void OnPreviewWorldPointChanged(Vector3 oldPoint, Vector3 newPoint)
        {
            _indicator.position = newPoint + Vector3.up * GroundYOffset;
        }

        private void OnExplosionRadiusChanged(float oldRadius, float newRadius)
        {
            ApplyRadiusScale();
        }

        private void OneOffSyncFromState()
        {
            _indicator.gameObject.SetActive(_previewVisible.Value);
            OnPreviewWorldPointChanged(default, _previewWorldPoint.Value);
            ApplyRadiusScale();
        }

        private void ApplyRadiusScale()
        {
            float padding = _explosionRadius.Value / ReferenceRadius * PaddingMult;
            float diameter = _explosionRadius.Value * 2f + padding;
            
            _indicator.localScale = new Vector3(diameter, diameter, 1f);
        }

        public override void Cleanup(Entity entity)
        {
            base.Cleanup(entity);

            _visibleDisposable?.Dispose();
            _pointDisposable?.Dispose();
            _radiusDisposable?.Dispose();
            _explosionRequestedDisposable?.Dispose();

            Destroy(_indicator.gameObject);
        }
    }
}