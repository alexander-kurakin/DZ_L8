using Assets._Project.Develop.Runtime.Configs.Gameplay.Throw;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.ThrowFeature
{
    public class ThrownProjectileWatchSystem : IInitializableSystem, IUpdatableSystem, IDisposableSystem
    {
        private readonly ThrowChargeConfig _throwChargeConfig;
        private readonly EntitiesLifeContext _entitiesLifeContext;

        private ReactiveVariable<bool> _isWatchingThrownProjectile;
        private ReactiveVariable<float> _throwPostImpactAimLockRemainingTime;
        private ReactiveVariable<Vector3> _rotationDirection;
        private ReactiveEvent<ThrowReleaseData> _throwReleased;
        private IDisposable _throwReleasedSubscription;
        private IDisposable _projectileImpactedSubscription;
        private Entity _watchedProjectile;

        public ThrownProjectileWatchSystem(ThrowChargeConfig throwChargeConfig, EntitiesLifeContext entitiesLifeContext)
        {
            _throwChargeConfig = throwChargeConfig;
            _entitiesLifeContext = entitiesLifeContext;
        }

        public void OnInit(Entity entity)
        {
            _isWatchingThrownProjectile = entity.IsWatchingThrownProjectile;
            _throwPostImpactAimLockRemainingTime = entity.ThrowPostImpactAimLockRemainingTime;
            _rotationDirection = entity.RotationDirection;
            _throwReleased = entity.ThrowReleased;
            _throwReleasedSubscription = _throwReleased.Subscribe(OnThrowReleased);
            _entitiesLifeContext.Released += OnEntityReleased;
        }

        public void OnUpdate(float deltaTime)
        {
            if (_throwPostImpactAimLockRemainingTime.Value <= 0f)
                return;

            _throwPostImpactAimLockRemainingTime.Value = Mathf.Max(0f, _throwPostImpactAimLockRemainingTime.Value - deltaTime);
        }

        public void OnDispose()
        {
            _throwReleasedSubscription?.Dispose();
            _projectileImpactedSubscription?.Dispose();
            _entitiesLifeContext.Released -= OnEntityReleased;
        }

        private void OnThrowReleased(ThrowReleaseData data)
        {
            Entity projectile = data.Projectile;

            if (projectile == null)
                return;

            ClearWatchedProjectile();

            _watchedProjectile = projectile;
            _isWatchingThrownProjectile.Value = true;
            ApplyHorizontalThrowDirection(data.Direction);

            if (projectile.TryGetProjectileImpacted(out ReactiveEvent<Vector3> projectileImpacted) == true)
                _projectileImpactedSubscription = projectileImpacted.Subscribe(OnProjectileImpacted);
        }

        private void OnProjectileImpacted(Vector3 impactPoint)
        {
            FinishWatchingProjectile();
            _throwPostImpactAimLockRemainingTime.Value = _throwChargeConfig.PostImpactAimLockSeconds;
        }

        private void OnEntityReleased(Entity entity)
        {
            if (_watchedProjectile != entity)
                return;

            FinishWatchingProjectile();
        }

        private void FinishWatchingProjectile()
        {
            _isWatchingThrownProjectile.Value = false;
            ClearWatchedProjectile();
        }

        private void ClearWatchedProjectile()
        {
            _projectileImpactedSubscription?.Dispose();
            _projectileImpactedSubscription = null;
            _watchedProjectile = null;
        }

        private void ApplyHorizontalThrowDirection(Vector3 throwDirection)
        {
            Vector3 horizontalDirection = new Vector3(throwDirection.x, 0f, throwDirection.z);

            if (horizontalDirection.sqrMagnitude <= 0f)
                return;

            _rotationDirection.Value = horizontalDirection.normalized;
        }
    }
}
