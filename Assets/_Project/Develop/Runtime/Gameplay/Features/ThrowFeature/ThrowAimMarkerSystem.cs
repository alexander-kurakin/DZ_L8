using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Gameplay.Features.ProjectileFeature;
using Assets._Project.Develop.Runtime.Utilities.AssetsManagment;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.ThrowFeature
{
    public class ThrowAimMarkerSystem : IInitializableSystem, IDisposableSystem
    {
        private const string MARKER_PREFAB_PATH = "Entities/ThrowAimMarker";
        private const float MARKER_OFFSET_TOWARDS_PLAYER = 0.5f;

        private readonly ResourcesAssetsLoader _resourcesAssetsLoader;

        private ReactiveEvent<ThrowReleaseData> _throwReleased;
        private IDisposable _throwReleasedSubscription;
        private IDisposable _projectileImpactedSubscription;
        private GameObject _currentMarker;

        public ThrowAimMarkerSystem(ResourcesAssetsLoader resourcesAssetsLoader)
        {
            _resourcesAssetsLoader = resourcesAssetsLoader;
        }

        public void OnInit(Entity entity)
        {
            _throwReleased = entity.ThrowReleased;
            _throwReleasedSubscription = _throwReleased.Subscribe(OnThrowReleased);
        }

        public void OnDispose()
        {
            _throwReleasedSubscription?.Dispose();
            _projectileImpactedSubscription?.Dispose();
            DestroyCurrentMarker();
        }

        private void OnThrowReleased(ThrowReleaseData data)
        {
            DestroyCurrentMarker();
            _projectileImpactedSubscription?.Dispose();
            _projectileImpactedSubscription = null;

            Entity projectile = data.Projectile;

            if (projectile == null)
                return;

            ProjectileImpacted projectileImpacted = projectile.GetComponent<ProjectileImpacted>();

            if (projectileImpacted == null)
                return;

            _projectileImpactedSubscription = projectileImpacted.Value.Subscribe(OnProjectileImpacted);
        }

        private void OnProjectileImpacted(Vector3 impactPoint)
        {
            Vector3 markerPosition = GetMarkerPosition(impactPoint);

            GameObject markerPrefab = _resourcesAssetsLoader.Load<GameObject>(MARKER_PREFAB_PATH);
            _currentMarker = Object.Instantiate(markerPrefab, markerPosition, Quaternion.identity);
        }

        private Vector3 GetMarkerPosition(Vector3 aimPoint)
        {
            Camera camera = Camera.main;

            if (camera == null)
                return aimPoint;

            Vector3 alongRayTowardsPlayer = camera.transform.position - aimPoint;

            if (alongRayTowardsPlayer.sqrMagnitude <= 0f)
                return aimPoint;

            return aimPoint + alongRayTowardsPlayer.normalized * MARKER_OFFSET_TOWARDS_PLAYER;
        }

        private void DestroyCurrentMarker()
        {
            if (_currentMarker == null)
                return;

            Object.Destroy(_currentMarker);
            _currentMarker = null;
        }
    }
}
