using System;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using Assets._Project.Develop.Runtime.Gameplay.Features.Ability;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.LeftClickAbilityPreview
{
    public class LeftClickAbilityView : EntityView
    {
        [SerializeField] private Transform _indicatorPrefab;

        private Transform _indicator;
        private Transform _cooldownFillIndicator;
        private Material _cooldownFillMaterial;

        private ReactiveVariable<Vector3> _previewWorldPoint;
        private ReactiveVariable<bool> _previewVisible;
        private ReactiveVariable<float> _previewIndicatorDiameter;
        private ReactiveVariable<float> _cooldownFill;

        private IDisposable _visibleDisposable;
        private IDisposable _pointDisposable;
        private IDisposable _diameterDisposable;
        private IDisposable _cooldownFillDisposable;

        protected override void OnEntityStartedWork(Entity entity)
        {
            _previewWorldPoint = entity.LeftClickPreviewWorldPoint;
            _previewVisible = entity.LeftClickPreviewVisible;
            _previewIndicatorDiameter = entity.LeftClickPreviewIndicatorDiameter;
            _cooldownFill = entity.LeftClickPreviewCooldownFill;

            InitIndicator();
            InitCooldownFillOverlay();
            ApplyIndicatorScale();
            ApplyCooldownFill(_cooldownFill.Value);

            _visibleDisposable = _previewVisible.Subscribe(OnPreviewVisibleChanged);
            _pointDisposable = _previewWorldPoint.Subscribe(OnPreviewWorldPointChanged);
            _diameterDisposable = _previewIndicatorDiameter.Subscribe(OnPreviewIndicatorDiameterChanged);
            _cooldownFillDisposable = _cooldownFill.Subscribe(OnCooldownFillChanged);
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
            BuildingBuffCastAbilityConfig config = LmbFrostProjectileService.Instance?.Config;
            float cooldownFillYOffset = config != null ? config.CooldownFillYOffset : 0f;
            Color cooldownFillColor = config != null ? config.CooldownFillColor : Color.red;

            _cooldownFillIndicator = Instantiate(_indicatorPrefab, _indicator);
            _cooldownFillIndicator.localPosition = new Vector3(0f, cooldownFillYOffset, 0f);
            _cooldownFillIndicator.localRotation = Quaternion.identity;
            _cooldownFillIndicator.localScale = Vector3.one;

            Renderer cooldownRenderer = _cooldownFillIndicator.GetComponent<Renderer>();
            _cooldownFillMaterial = cooldownRenderer.material;
            _cooldownFillMaterial.color = cooldownFillColor;

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
            BuildingBuffCastAbilityConfig config = LmbFrostProjectileService.Instance?.Config;
            float referenceDiameter = config != null ? config.PreviewReferenceDiameter : 1f;
            float paddingMultiplier = config != null ? config.PreviewPaddingMultiplier : 0f;
            float diameter = _previewIndicatorDiameter.Value;
            float padding = diameter / referenceDiameter * paddingMultiplier;
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

            if (_cooldownFillMaterial != null)
                Destroy(_cooldownFillMaterial);

            Destroy(_indicator.gameObject);
        }
    }
}
