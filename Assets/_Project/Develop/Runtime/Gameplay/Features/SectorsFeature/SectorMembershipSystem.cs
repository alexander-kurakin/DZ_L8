using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Gameplay.Features.PlantCombatFeature;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature
{
    public class SectorMembershipSystem : IInitializableSystem, IUpdatableSystem
    {
        private readonly SectorMembershipService _sectorMembershipService;

        private ReactiveVariable<SectorId> _currentSector;
        private Transform _trackedTransform;
        private Rigidbody _rigidbody;
        private bool _isFlyingEnemy;

        public SectorMembershipSystem(SectorMembershipService sectorMembershipService)
        {
            _sectorMembershipService = sectorMembershipService;
        }

        public void OnInit(Entity entity)
        {
            _currentSector = entity.CurrentSector;
            _isFlyingEnemy = entity.HasComponent<FlyingEnemy>();

            if (entity.TryGetTransform(out Transform transform))
                _trackedTransform = transform;

            if (entity.TryGetRigidbody(out Rigidbody rigidbody))
                _rigidbody = rigidbody;
        }

        public void OnUpdate(float deltaTime)
        {
            if (_trackedTransform == null && _rigidbody == null)
                return;

            Vector3 worldPosition = _trackedTransform != null
                ? _trackedTransform.position
                : _rigidbody.position;

            SectorId resolvedSector = _isFlyingEnemy
                ? _sectorMembershipService.ResolveFlyingEnemyFromWorldPosition(worldPosition)
                : _sectorMembershipService.ResolveFromWorldPosition(worldPosition);

            if (_currentSector.Value.Equals(resolvedSector))
                return;

            _currentSector.Value = resolvedSector;
        }
    }
}
