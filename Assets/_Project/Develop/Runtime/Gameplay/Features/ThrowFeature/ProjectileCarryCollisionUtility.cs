using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.ThrowFeature
{
    public static class ProjectileCarryCollisionUtility
    {
        public static void SetColliderEnabled(Entity projectile, bool isEnabled)
        {
            if (projectile.TryGetSphereBodyCollider(out SphereCollider sphereCollider) == false)
                return;

            sphereCollider.enabled = isEnabled;
        }

        public static void SetOwnerCollisionsIgnored(Entity projectile, Entity owner, bool ignore)
        {
            if (projectile.TryGetSphereBodyCollider(out SphereCollider projectileCollider) == false)
                return;

            if (owner == null || owner.TryGetTransform(out Transform ownerTransform) == false)
                return;

            Collider[] ownerColliders = ownerTransform.GetComponentsInChildren<Collider>(true);

            for (int colliderIndex = 0; colliderIndex < ownerColliders.Length; colliderIndex++)
            {
                Collider ownerCollider = ownerColliders[colliderIndex];

                if (ownerCollider == null || ownerCollider.enabled == false)
                    continue;

                Physics.IgnoreCollision(projectileCollider, ownerCollider, ignore);
            }
        }
    }
}
