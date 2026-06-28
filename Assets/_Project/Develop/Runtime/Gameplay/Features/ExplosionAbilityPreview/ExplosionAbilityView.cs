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
        private const float ReferenceDiameter = 10f;
        private const float PaddingMult = 2f;
        private const float VFXscalingMult = 0.85f;
        private const float COOLDOWN_FILL_Y_OFFSET = 0.01f;

        private static readonly Color COOLDOWN_FILL_COLOR = new Color(1f, 0.15f, 0.15f, 0.6f);

        [SerializeField] private Transform _indicatorPrefab;
        [SerializeField] private ParticleSystem _castVfxPrefab;
        [SerializeField] private GameSoundsIDs _castSound;
        [SerializeField] private AudioSource _localAudioSource;

        private Transform _indicator;
        private Transform _cooldownFillIndicator;
        private Material _cooldownFillMaterial;
        private ParticleSystem _castVfxPrefabInstance;

        private ReactiveVariable<Vector3> _previewWorldPoint;
        private ReactiveVariable<bool> _previewVisible;
        private ReactiveVariable<float> _previewIndicatorDiameter;
        private ReactiveVariable<float> _cooldownFill;
        private Entity _explodeAbility;

        private IDisposable _visibleDisposable;
        private IDisposable _pointDisposable;
        private IDisposable _diameterDisposable;
        private IDisposable _cooldownFillDisposable;
        private IDisposable _explosionRequestedDisposable;

        protected override void OnEntityStartedWork(Entity entity)
        {
            _previewWorldPoint = entity.ExplosionPreviewWorldPoint;
            _previewVisible = entity.ExplosionPreviewVisible;
            _previewIndicatorDiameter = entity.ExplosionPreviewIndicatorDiameter;
            _cooldownFill = entity.ExplosionPreviewCooldownFill;

            if (entity.AbilityUserAllAbilities.TryGetValue(AbilityType.ExplodeAtPoint, out _explodeAbility))
            {
                InitIndicator();
                InitCooldownFillOverlay();
                ApplyIndicatorScale();
                ApplyCooldownFill(_cooldownFill.Value);

                _visibleDisposable = _previewVisible.Subscribe(OnPreviewVisibleChanged);
                _pointDisposable = _previewWorldPoint.Subscribe(OnPreviewWorldPointChanged);
                _diameterDisposable = _previewIndicatorDiameter.Subscribe(OnPreviewIndicatorDiameterChanged);
                _cooldownFillDisposable = _cooldownFill.Subscribe(OnCooldownFillChanged);
                _explosionRequestedDisposable = _explodeAbility.DealAreaImpactDamageRequest.Subscribe(OnExplosionRequested);
            }
        }

        private float GetVFXScale(float indicatorDiameter)
        {
            return VFXscalingMult * (indicatorDiameter / ReferenceDiameter);
        }

        private void OnExplosionRequested(Vector3 worldPoint)
        {
            _castVfxPrefabInstance = Instantiate(_castVfxPrefab, worldPoint, Quaternion.identity);

            float scale = GetVFXScale(_previewIndicatorDiameter.Value);
            _castVfxPrefabInstance.transform.localScale = new Vector3(scale, 1f, scale);

            GameSoundsService.PlayOneShot(_castSound, _localAudioSource);
        }

        private void OnDrawGizmos()
        {
            if (_indicator == null || _previewIndicatorDiameter == null || _previewVisible.Value == false)
                return;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_previewWorldPoint.Value, _previewIndicatorDiameter.Value * 0.5f);
        }

        private void InitIndicator()
        {
            _indicator = Instantiate(_indicatorPrefab);
            _indicator.gameObject.SetActive(false);
        }

        private void InitCooldownFillOverlay()
        {
            _cooldownFillIndicator = Instantiate(_indicatorPrefab, _indicator);
            _cooldownFillIndicator.localPosition = new Vector3(0f, COOLDOWN_FILL_Y_OFFSET, 0f);
            _cooldownFillIndicator.localRotation = Quaternion.identity;
            _cooldownFillIndicator.localScale = Vector3.one;

            Renderer cooldownRenderer = _cooldownFillIndicator.GetComponent<Renderer>();
            _cooldownFillMaterial = cooldownRenderer.material;
            _cooldownFillMaterial.color = COOLDOWN_FILL_COLOR;

            _cooldownFillIndicator.gameObject.SetActive(false);
        }

        private void OnPreviewVisibleChanged(bool oldVisible, bool newVisible)
        {
            _indicator.gameObject.SetActive(newVisible);
        }

        private void OnPreviewWorldPointChanged(Vector3 oldPoint, Vector3 newPoint)
        {
            _indicator.position = newPoint;
        }

        private void OnPreviewIndicatorDiameterChanged(float oldDiameter, float newDiameter)
        {
            ApplyIndicatorScale();
        }

        private void OnCooldownFillChanged(float oldFill, float newFill)
        {
            ApplyCooldownFill(newFill);
        }

        private void ApplyIndicatorScale()
        {
            float diameter = _previewIndicatorDiameter.Value;
            float padding = diameter / ReferenceDiameter * PaddingMult;
            float visualDiameter = diameter + padding;

            _indicator.localScale = new Vector3(visualDiameter, visualDiameter, 1f);
        }

        private void ApplyCooldownFill(float fillAmount)
        {
            if (_cooldownFillIndicator == null)
                return;

            if (fillAmount <= 0f)
            {
                _cooldownFillIndicator.gameObject.SetActive(false);
                return;
            }

            _cooldownFillIndicator.gameObject.SetActive(true);
            _cooldownFillIndicator.localScale = new Vector3(fillAmount, fillAmount, 1f);
        }

        public override void Cleanup(Entity entity)
        {
            base.Cleanup(entity);

            _visibleDisposable?.Dispose();
            _pointDisposable?.Dispose();
            _diameterDisposable?.Dispose();
            _cooldownFillDisposable?.Dispose();
            _explosionRequestedDisposable?.Dispose();

            if (_cooldownFillMaterial != null)
                Destroy(_cooldownFillMaterial);

            Destroy(_indicator.gameObject);
        }
    }
}
