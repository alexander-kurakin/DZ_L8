using System;
using _Project.Develop.Runtime.Gameplay.Features.LeftClickAbilityPreview;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using Assets._Project.Develop.Runtime.Gameplay.Features.JuiceFeature;
using DG.Tweening;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.TowerWalker
{
    [RequireComponent(typeof(Animator))]
    public class MagicCastingView : EntityView
    {
        private const float MIN_DIRECTION_SQR_MAGNITUDE = 0.0001f;

        private readonly int MagicCastedHash = Animator.StringToHash("MagicCasted");

        [SerializeField] private Animator _animator;
        [SerializeField] private Transform _projectileSpawnPoint;

        private IDisposable _magicCastRequest;
        private LmbFrostProjectileService _lmbFrostProjectileService;
        private Tween _castWindupTween;

        private void OnValidate()
        {
            _animator ??= GetComponent<Animator>();

            if (_projectileSpawnPoint != null)
                return;

            _projectileSpawnPoint = transform;
        }

        protected override void OnEntityStartedWork(Entity entity)
        {
            _lmbFrostProjectileService = LmbFrostProjectileService.Instance;
            _magicCastRequest = entity.MagicCastRequestedEvent.Subscribe(OnMagicCastRequested);

            if (_lmbFrostProjectileService != null)
                _lmbFrostProjectileService.RegisterProjectileLauncher(LaunchProjectile);
        }

        public override void Cleanup(Entity entity)
        {
            base.Cleanup(entity);

            _castWindupTween?.Kill();
            _magicCastRequest?.Dispose();
            _lmbFrostProjectileService?.ClearProjectileLauncher();
        }

        private void OnMagicCastRequested(Vector3 worldPoint)
        {
            if (_lmbFrostProjectileService != null
                && _lmbFrostProjectileService.HasQueuedProjectileLaunch() == false)
                return;

            _animator.SetTrigger(MagicCastedHash);

            BuildingBuffCastAbilityConfig config = _lmbFrostProjectileService?.Config;
            float castWindupSeconds = config != null ? config.CastWindupSeconds : 0f;

            _castWindupTween?.Kill();
            _castWindupTween = DOVirtual
                .DelayedCall(castWindupSeconds, OnCastWindupComplete)
                .Play();
        }

        private void OnCastWindupComplete()
        {
            _lmbFrostProjectileService?.TryLaunchQueuedProjectile();
        }

        private void LaunchProjectile(Vector3 targetWorldPoint)
        {
            if (_lmbFrostProjectileService == null)
                return;

            GameObject projectilePrefab = _lmbFrostProjectileService.ProjectilePrefab;

            if (projectilePrefab == null)
            {
                _lmbFrostProjectileService.NotifyImpact(targetWorldPoint);
                return;
            }

            BuildingBuffCastAbilityConfig config = _lmbFrostProjectileService.Config;
            Transform spawnTransform = _projectileSpawnPoint != null ? _projectileSpawnPoint : transform;
            float projectileSpawnHeight = config != null ? config.ProjectileSpawnHeight : 0f;
            Vector3 startPosition = spawnTransform.position + Vector3.up * projectileSpawnHeight;
            Vector3 flightVector = targetWorldPoint - startPosition;

            if (flightVector.sqrMagnitude < MIN_DIRECTION_SQR_MAGNITUDE)
            {
                _lmbFrostProjectileService.NotifyImpact(targetWorldPoint);
                return;
            }

            float projectileYawOffset = config != null ? config.ProjectileYawOffsetDegrees : 0f;
            Quaternion rotation = Quaternion.LookRotation(flightVector.normalized, Vector3.up)
                * Quaternion.Euler(0f, projectileYawOffset, 0f);
            GameObject projectileInstance = GameplayVfxUtility.SpawnAt(
                projectilePrefab,
                startPosition,
                rotation,
                null,
                _lmbFrostProjectileService.ProjectileScale);

            GameplayVfxUtility.PlayMovingProjectileParticles(projectileInstance);

            projectileInstance.layer = spawnTransform.gameObject.layer;
            SetLayerRecursively(projectileInstance, spawnTransform.gameObject.layer);

            float flightDistance = flightVector.magnitude;
            float flightDuration = flightDistance / _lmbFrostProjectileService.ProjectileSpeed;

            projectileInstance.transform
                .DOMove(targetWorldPoint, flightDuration)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    if (projectileInstance != null)
                        Destroy(projectileInstance);

                    _lmbFrostProjectileService.NotifyImpact(targetWorldPoint);
                })
                .Play();
        }

        private static void SetLayerRecursively(GameObject root, int layer)
        {
            root.layer = layer;

            Transform rootTransform = root.transform;

            for (int childIndex = 0; childIndex < rootTransform.childCount; childIndex++)
                SetLayerRecursively(rootTransform.GetChild(childIndex).gameObject, layer);
        }
    }
}
