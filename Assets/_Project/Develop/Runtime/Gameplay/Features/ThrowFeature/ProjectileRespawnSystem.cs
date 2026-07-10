using Assets._Project.Develop.Runtime.Configs.Gameplay.Throw;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.ThrowFeature
{
    public class ProjectileRespawnSystem : IInitializableSystem, IUpdatableSystem, IDisposableSystem
    {
        private readonly EntitiesFactory _entitiesFactory;
        private readonly ThrowChargeConfig _throwChargeConfig;

        private Entity _heroEntity;
        private Transform _shootingPoint;
        private ReactiveVariable<bool> _isProjectileInHand;
        private ReactiveVariable<Entity> _currentProjectile;
        private ReactiveEvent<ThrowReleaseData> _throwReleased;
        private IDisposable _throwReleasedSubscription;

        private float _respawnCooldownRemaining;

        public ProjectileRespawnSystem(EntitiesFactory entitiesFactory, ThrowChargeConfig throwChargeConfig)
        {
            _entitiesFactory = entitiesFactory;
            _throwChargeConfig = throwChargeConfig;
        }

        public void OnInit(Entity entity)
        {
            _heroEntity = entity;
            _shootingPoint = entity.ShootingPoint;
            _isProjectileInHand = entity.IsProjectileInHand;
            _currentProjectile = entity.CurrentProjectile;
            _throwReleased = entity.ThrowReleased;
            _throwReleasedSubscription = _throwReleased.Subscribe(OnThrowReleased);

            if (_isProjectileInHand.Value == false)
                EquipProjectileInHand();
        }

        public void OnDispose()
        {
            _throwReleasedSubscription?.Dispose();
        }

        public void OnUpdate(float deltaTime)
        {
            if (_respawnCooldownRemaining > 0f)
            {
                _respawnCooldownRemaining -= deltaTime;

                if (_respawnCooldownRemaining > 0f)
                    return;
            }

            if (_isProjectileInHand.Value == true)
                return;

            EquipProjectileInHand();
        }

        private void OnThrowReleased(ThrowReleaseData data)
        {
            _respawnCooldownRemaining = _throwChargeConfig.ProjectileRespawnCooldownSeconds;
        }

        private void EquipProjectileInHand()
        {
            Entity projectileEntity = _entitiesFactory.CreateProjectile(_shootingPoint.position, _heroEntity);
            Transform projectileTransform = projectileEntity.Transform;

            projectileTransform.SetParent(_shootingPoint, worldPositionStays: false);
            projectileTransform.localPosition = Vector3.zero;
            projectileTransform.localRotation = Quaternion.identity;

            ProjectileCarryCollisionUtility.SetColliderEnabled(projectileEntity, false);
            ProjectileCarryCollisionUtility.SetOwnerCollisionsIgnored(projectileEntity, _heroEntity, true);

            _currentProjectile.Value = projectileEntity;
            _isProjectileInHand.Value = true;
        }
    }
}
