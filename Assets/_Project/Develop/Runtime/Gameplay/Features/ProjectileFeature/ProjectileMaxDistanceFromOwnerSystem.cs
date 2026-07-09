using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.ProjectileFeature
{
    public class ProjectileMaxDistanceFromOwnerSystem : IInitializableSystem, IUpdatableSystem
    {
        private readonly float _maxDistanceSqr;

        private ReactiveVariable<bool> _hasCollided;
        private Rigidbody _rigidbody;
        private Transform _ownerTransform;
        private Transform _projectileTransform;

        public ProjectileMaxDistanceFromOwnerSystem(float maxDistanceFromOwner)
        {
            _maxDistanceSqr = maxDistanceFromOwner * maxDistanceFromOwner;
        }

        public void OnInit(Entity entity)
        {
            _hasCollided = entity.HasCollided;
            _rigidbody = entity.Rigidbody;
            _projectileTransform = entity.Transform;

            Entity owner = entity.ProjectileOwner;

            if (owner != null && owner.TryGetTransform(out Transform ownerTransform) == true)
                _ownerTransform = ownerTransform;
        }

        public void OnUpdate(float deltaTime)
        {
            if (_hasCollided.Value == true)
                return;

            if (_ownerTransform == null)
                return;

            Vector3 offset = _projectileTransform.position - _ownerTransform.position;

            if (offset.sqrMagnitude <= _maxDistanceSqr)
                return;

            _hasCollided.Value = true;
            _rigidbody.useGravity = true;
        }
    }
}
