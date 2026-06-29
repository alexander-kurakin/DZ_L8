using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Attack.Shoot
{
    public class ProjectileOffScreenBoundsSystem : IInitializableSystem, IUpdatableSystem
    {
        private readonly Camera _camera;
        private readonly SectorRegistryService _sectorRegistryService;
        private readonly ProjectileBoundsConfig _config;

        private Entity _entity;
        private ReactiveVariable<bool> _isDead;
        private Vector3 _spawnPosition;
        private float _aliveSeconds;
        private float _maxTravelDistanceSqr;

        public ProjectileOffScreenBoundsSystem(
            Camera camera,
            SectorRegistryService sectorRegistryService,
            ProjectileBoundsConfig config)
        {
            _camera = camera;
            _sectorRegistryService = sectorRegistryService;
            _config = config;
        }

        public void OnInit(Entity entity)
        {
            _entity = entity;
            _isDead = entity.IsDead;
            _aliveSeconds = 0f;
            _maxTravelDistanceSqr = ResolveMaxTravelDistanceSqr();

            if (entity.TryGetTransform(out Transform projectileTransform))
                _spawnPosition = projectileTransform.position;
        }

        public void OnUpdate(float deltaTime)
        {
            if (_isDead.Value)
                return;

            _aliveSeconds += deltaTime;

            if (_aliveSeconds >= _config.MaxLifetimeSeconds)
            {
                MarkDead();
                return;
            }

            if (_entity.TryGetTransform(out Transform projectileTransform) == false)
                return;

            if (HasExceededTravelDistance(projectileTransform.position))
            {
                MarkDead();
                return;
            }

            if (IsOutsidePlayableArena(projectileTransform.position))
            {
                MarkDead();
                return;
            }

            if (_camera == null)
                return;

            Vector3 viewportPoint = _camera.WorldToViewportPoint(projectileTransform.position);

            if (viewportPoint.z <= 0f)
            {
                MarkDead();
                return;
            }

            if (viewportPoint.x < -_config.ViewportMargin
                || viewportPoint.x > 1f + _config.ViewportMargin
                || viewportPoint.y < -_config.ViewportMargin
                || viewportPoint.y > 1f + _config.ViewportMargin)
            {
                MarkDead();
            }
        }

        private void MarkDead()
        {
            _isDead.Value = true;
        }

        private float ResolveMaxTravelDistanceSqr()
        {
            if (_sectorRegistryService.IsInitialized == false)
            {
                float fallbackDistance = _config.FallbackMaxTravelDistance;
                return fallbackDistance * fallbackDistance;
            }

            float maxTravelDistance = _sectorRegistryService.GridConfig.OuterBeltMaxRadius + _config.MaxTravelDistanceMargin;
            return maxTravelDistance * maxTravelDistance;
        }

        private bool HasExceededTravelDistance(Vector3 worldPosition)
        {
            Vector3 travelOffset = worldPosition - _spawnPosition;
            travelOffset.y = 0f;

            return travelOffset.sqrMagnitude > _maxTravelDistanceSqr;
        }

        private bool IsOutsidePlayableArena(Vector3 worldPosition)
        {
            if (_sectorRegistryService.IsInitialized == false)
                return false;

            Vector3 offsetFromCenter = worldPosition - _sectorRegistryService.Center;
            offsetFromCenter.y = 0f;
            float maxRadius = _sectorRegistryService.GridConfig.OuterBeltMaxRadius + _config.ArenaRadiusMargin;

            return offsetFromCenter.sqrMagnitude > maxRadius * maxRadius;
        }
    }
}
