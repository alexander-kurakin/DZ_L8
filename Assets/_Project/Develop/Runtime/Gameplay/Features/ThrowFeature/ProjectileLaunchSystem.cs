using Assets._Project.Develop.Runtime.Configs.Gameplay.Throw;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.ThrowFeature
{
    public class ProjectileLaunchSystem : IInitializableSystem, IDisposableSystem
    {
        private readonly ThrowChargeConfig _throwChargeConfig;

        private ReactiveEvent<ThrowReleaseData> _throwReleased;
        private IDisposable _throwReleasedSubscription;
        private Transform _throwReleasePoint;

        public ProjectileLaunchSystem(ThrowChargeConfig throwChargeConfig)
        {
            _throwChargeConfig = throwChargeConfig;
        }

        public void OnInit(Entity entity)
        {
            _throwReleased = entity.ThrowReleased;
            _throwReleasePoint = entity.ThrowReleasePoint;
            _throwReleasedSubscription = _throwReleased.Subscribe(OnThrowReleased);
        }

        public void OnDispose()
        {
            _throwReleasedSubscription?.Dispose();
        }

        private void OnThrowReleased(ThrowReleaseData data)
        {
            Entity projectile = data.Projectile;

            if (projectile == null)
                return;

            projectile.Transform.SetParent(null, worldPositionStays: true);

            if (_throwReleasePoint != null)
                projectile.Transform.position = _throwReleasePoint.position;

            Vector3 direction = data.Direction.sqrMagnitude > 0f ? data.Direction.normalized : Vector3.forward;
            float speed = _throwChargeConfig.EvaluateThrowSpeed(data.Power);

            projectile.MoveDirection.Value = direction;
            projectile.RotationDirection.Value = direction;
            projectile.MoveSpeed.Value = speed;
            projectile.ProjectileSpeed.Value = speed;
            projectile.ProjectileDamage.Value = data.Power;
            projectile.HasCollided.Value = true;

            Rigidbody rigidbody = projectile.Rigidbody;
            rigidbody.isKinematic = false;
            rigidbody.useGravity = true;
            rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rigidbody.velocity = direction * speed;
        }
    }
}
