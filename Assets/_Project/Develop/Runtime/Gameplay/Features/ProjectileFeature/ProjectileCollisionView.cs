using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.ProjectileFeature
{
    [RequireComponent(typeof(Rigidbody))]
    public class ProjectileCollisionView : EntityView
    {
        private ReactiveVariable<bool> _hasCollided;
        private ReactiveEvent<Vector3> _projectileImpacted;
        private Entity _projectileOwner;
        private Rigidbody _rigidbody;
        private bool _hasInvokedProjectileImpacted;

        protected override void OnEntityStartedWork(Entity entity)
        {
            _hasCollided = entity.HasCollided;
            _projectileImpacted = entity.GetComponent<ProjectileImpacted>().Value;
            _projectileOwner = entity.ProjectileOwner;
            _rigidbody = entity.Rigidbody;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (IsOwnerCollider(collision.collider) == true)
                return;

            if (_hasInvokedProjectileImpacted == false)
            {
                if (collision.contactCount > 0)
                {
                    _hasInvokedProjectileImpacted = true;
                    _projectileImpacted.Invoke(collision.GetContact(0).point);
                }
            }

            if (_hasCollided.Value == true)
                return;

            _hasCollided.Value = true;
            _rigidbody.useGravity = true;
        }

        private bool IsOwnerCollider(Collider collider)
        {
            if (_projectileOwner == null)
                return false;

            if (_projectileOwner.TryGetTransform(out Transform ownerTransform) == false)
                return false;

            return collider.transform.IsChildOf(ownerTransform);
        }
    }
}
