using System;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using Assets._Project.Develop.Runtime.Gameplay.Features.JuiceFeature;
using DG.Tweening;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _Project.Develop.Runtime.Gameplay.Features.LeftClickAbilityPreview
{
    public class LmbFrostProjectileService
    {
        public static LmbFrostProjectileService Instance { get; private set; }

        private Action<Vector3> _projectileLauncher;
        private Action<Vector3> _impactHandler;
        private BuildingBuffCastAbilityConfig _config;
        private GameObject _activeTargetOrbsInstance;
        private Tween _clearOrbsTween;

        private Vector3? _queuedTargetWorldPoint;

        public LmbFrostProjectileService()
        {
            Instance = this;
        }

        public BuildingBuffCastAbilityConfig Config => _config;

        public GameObject ProjectilePrefab => _config?.FrostProjectilePrefab;

        public float ProjectileSpeed => _config != null ? _config.FrostProjectileSpeed : 0f;

        public float ProjectileScale => _config != null ? _config.FrostProjectileScale : 1f;

        public bool HasQueuedProjectileLaunch()
        {
            return _queuedTargetWorldPoint.HasValue;
        }

        public void Configure(BuildingBuffCastAbilityConfig config)
        {
            _config = config;
        }

        public void RegisterProjectileLauncher(Action<Vector3> launcher)
        {
            _projectileLauncher = launcher;
        }

        public void RegisterImpactHandler(Action<Vector3> handler)
        {
            _impactHandler = handler;
        }

        public void ClearProjectileLauncher()
        {
            _projectileLauncher = null;
        }

        public void ClearImpactHandler()
        {
            _impactHandler = null;
        }

        public void QueueProjectileLaunch(Vector3 targetWorldPoint)
        {
            _queuedTargetWorldPoint = targetWorldPoint;
        }

        public void ShowTargetOrbs(GameObject orbsPrefab, Vector3 worldPoint, float scale)
        {
            CancelScheduledOrbsClear();
            ClearTargetOrbs();

            if (orbsPrefab == null)
                return;

            _activeTargetOrbsInstance = GameplayVfxUtility.SpawnAt(
                orbsPrefab,
                worldPoint,
                Quaternion.identity,
                null,
                scale);
        }

        public void ClearTargetOrbs()
        {
            CancelScheduledOrbsClear();

            if (_activeTargetOrbsInstance == null)
                return;

            Object.Destroy(_activeTargetOrbsInstance);
            _activeTargetOrbsInstance = null;
        }

        public void ClearQueuedProjectileLaunch()
        {
            _queuedTargetWorldPoint = null;
            ClearTargetOrbs();
        }

        public void TryLaunchQueuedProjectile()
        {
            if (_queuedTargetWorldPoint.HasValue == false)
                return;

            Vector3 targetWorldPoint = _queuedTargetWorldPoint.Value;
            _queuedTargetWorldPoint = null;
            RequestProjectileLaunch(targetWorldPoint);
        }

        public void RequestProjectileLaunch(Vector3 targetWorldPoint)
        {
            if (_projectileLauncher != null)
            {
                _projectileLauncher.Invoke(targetWorldPoint);
                return;
            }

            NotifyImpact(targetWorldPoint);
        }

        public void NotifyImpact(Vector3 impactWorldPoint)
        {
            _impactHandler?.Invoke(impactWorldPoint);
            ScheduleClearTargetOrbsAfterImpact();
        }

        private void ScheduleClearTargetOrbsAfterImpact()
        {
            CancelScheduledOrbsClear();

            if (_activeTargetOrbsInstance == null)
                return;

            if (_config == null)
            {
                ClearTargetOrbs();
                return;
            }

            _clearOrbsTween = DOVirtual
                .DelayedCall(_config.FrostTargetOrbsLingerAfterImpactSeconds, ClearTargetOrbs)
                .Play();
        }

        private void CancelScheduledOrbsClear()
        {
            _clearOrbsTween?.Kill();
            _clearOrbsTween = null;
        }
    }
}
