using Assets._Project.Develop.Runtime.Configs.Gameplay.Essence;
using DG.Tweening;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.EssenceFeature
{
    public class EssencePickupView : MonoBehaviour
    {
        private EssenceConfig _essenceConfig;
        private float _remainingHoverLockSeconds;
        private Transform _visualRoot;
        private SphereCollider _sphereCollider;
        private GameObject _vacuumTrailPrefab;
        private GameObject _vacuumTrailInstance;
        private Tween _vacuumScaleTween;
        private Tween _hoverPopScaleTween;

        public int Amount { get; private set; }

        public bool IsVacuuming { get; private set; }

        public bool CanAcceptHover { get; private set; }

        public void Initialize(
            int amount,
            Vector3 worldPosition,
            EssenceConfig essenceConfig,
            GameObject vacuumTrailPrefab)
        {
            _essenceConfig = essenceConfig;
            Amount = amount;
            CanAcceptHover = false;
            _remainingHoverLockSeconds = essenceConfig.HoverUnlockDelay;
            _vacuumTrailPrefab = vacuumTrailPrefab;
            _visualRoot = transform.childCount > 0 ? transform.GetChild(0) : transform;

            transform.localScale = Vector3.one;
            _visualRoot.localScale = Vector3.one * essenceConfig.PickupGlowGroundScale;

            worldPosition.y += essenceConfig.PickupFloorOffset;
            transform.position = worldPosition;

            SetupVisualParticles(_visualRoot.gameObject);
            ConfigureCollider();
        }

        public void TickHoverLock(float deltaTime)
        {
            if (CanAcceptHover)
                return;

            _remainingHoverLockSeconds -= deltaTime;

            if (_remainingHoverLockSeconds > 0f)
                return;

            ActivateHoverReady();
        }

        public void ForceActivateHover()
        {
            ActivateHoverReady();
        }

        public void StartVacuuming()
        {
            if (IsVacuuming)
                return;

            IsVacuuming = true;
            AttachVacuumTrail();

            _hoverPopScaleTween?.Kill();
            _vacuumScaleTween?.Kill();

            float hoverScale = GetHoverScale();
            float pulseScale = GetVacuumPulseScale();
            _vacuumScaleTween = DOTween.Sequence()
                .Append(_visualRoot
                    .DOScale(Vector3.one * pulseScale, _essenceConfig.PickupVacuumPulseUpDurationSeconds)
                    .SetEase(Ease.OutBack, 1.4f, 0.35f))
                .Append(_visualRoot
                    .DOScale(Vector3.one * hoverScale, _essenceConfig.PickupVacuumSettleDurationSeconds)
                    .SetEase(Ease.OutCubic))
                .Play();
        }

        public Vector3 GetWorldPosition()
        {
            return transform.position;
        }

        public void MoveTowards(Vector3 targetPosition, float moveSpeed, float deltaTime)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * deltaTime);
        }

        private void AttachVacuumTrail()
        {
            if (_vacuumTrailPrefab == null)
                return;

            if (_vacuumTrailInstance != null)
                return;

            _vacuumTrailInstance = Instantiate(_vacuumTrailPrefab, transform);
            _vacuumTrailInstance.transform.localPosition = new Vector3(0f, _essenceConfig.PickupHoverColliderCenterY, 0f);
            _vacuumTrailInstance.transform.localRotation = Quaternion.identity;
            _vacuumTrailInstance.transform.localScale = Vector3.one * _essenceConfig.PickupVacuumTrailScale;

            PlayParticleSystemsInChildren(_vacuumTrailInstance);
        }

        private static void SetupVisualParticles(GameObject root)
        {
            ParticleSystem[] particleSystems = root.GetComponentsInChildren<ParticleSystem>(true);

            for (int index = 0; index < particleSystems.Length; index++)
            {
                ParticleSystem.MainModule mainModule = particleSystems[index].main;
                mainModule.scalingMode = ParticleSystemScalingMode.Hierarchy;
                particleSystems[index].Play(true);
            }
        }

        private static void PlayParticleSystemsInChildren(GameObject root)
        {
            ParticleSystem[] particleSystems = root.GetComponentsInChildren<ParticleSystem>(true);

            for (int index = 0; index < particleSystems.Length; index++)
                particleSystems[index].Play(true);
        }

        private float GetHoverScale()
        {
            return _essenceConfig.PickupGlowGroundScale * _essenceConfig.PickupHoverReadyScaleFactor;
        }

        private float GetVacuumPulseScale()
        {
            return _essenceConfig.PickupGlowGroundScale * _essenceConfig.PickupVacuumPulseScaleFactor;
        }

        private void ActivateHoverReady()
        {
            if (CanAcceptHover)
                return;

            CanAcceptHover = true;
            _remainingHoverLockSeconds = 0f;
            _sphereCollider.enabled = true;
            PlayHoverReadyPop();
        }

        private void PlayHoverReadyPop()
        {
            _hoverPopScaleTween?.Kill();
            _hoverPopScaleTween = _visualRoot
                .DOScale(Vector3.one * GetHoverScale(), _essenceConfig.PickupHoverReadyGrowDurationSeconds)
                .SetEase(Ease.OutCubic)
                .Play();
        }

        private void ConfigureCollider()
        {
            RemoveChildColliders();

            _sphereCollider = GetComponent<SphereCollider>();

            if (_sphereCollider == null)
                _sphereCollider = gameObject.AddComponent<SphereCollider>();

            _sphereCollider.center = new Vector3(0f, _essenceConfig.PickupHoverColliderCenterY, 0f);
            _sphereCollider.radius = _essenceConfig.PickupHoverColliderRadius;

            _sphereCollider.isTrigger = true;
            _sphereCollider.enabled = false;
        }

        private void RemoveChildColliders()
        {
            Collider[] colliders = GetComponentsInChildren<Collider>(true);

            for (int index = 0; index < colliders.Length; index++)
            {
                Collider collider = colliders[index];

                if (collider.gameObject == gameObject)
                    continue;

                Destroy(collider);
            }
        }

        private void OnDestroy()
        {
            _vacuumScaleTween?.Kill();
            _hoverPopScaleTween?.Kill();
        }
    }
}
