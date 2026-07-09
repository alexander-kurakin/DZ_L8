using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Utilities;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Sensors
{
    public class BodyContactsDetectingSystem : IInitializableSystem, IUpdatableSystem
    {
        private const int SPHERE_CAST_HITS_CAPACITY = 32;

        private Buffer<Collider> _contacts;
        private LayerMask _mask;
        private CapsuleCollider _capsuleBody;
        private SphereCollider _sphericalBody;
        private Rigidbody _rigidbody;
        private ColliderType _colliderType;
        private RaycastHit[] _sphereCastHits;
        private Vector3 _previousSphereCenter;
        private bool _hasPreviousSphereCenter;

        public BodyContactsDetectingSystem(ColliderType colliderType)
        {
            _colliderType = colliderType;
        }

        public void OnInit(Entity entity)
        {
            _contacts = entity.ContactCollidersBuffer;
            _mask = entity.ContactsDetectingMask;
            _sphereCastHits = new RaycastHit[SPHERE_CAST_HITS_CAPACITY];
            _hasPreviousSphereCenter = false;

            if (entity.TryGetRigidbody(out Rigidbody rigidbody) == true)
                _rigidbody = rigidbody;

            switch (_colliderType)
            {
                case ColliderType.Capsule:
                    _capsuleBody = entity.BodyCollider;
                    break;

                case ColliderType.Sphere:
                    _sphericalBody = entity.SphereBodyCollider;
                    break;
            }
        }

        public void OnUpdate(float deltaTime)
        {
            switch (_colliderType)
            {
                case ColliderType.Capsule:
                    OverlapCapsule();
                    RemoveSelfFromContacts(_capsuleBody);
                    break;

                case ColliderType.Sphere:
                    DetectSphereContacts(deltaTime);
                    RemoveSelfFromContacts(_sphericalBody);
                    break;
            }
        }

        private void DetectSphereContacts(float deltaTime)
        {
            Vector3 sphereCenter = GetSphereWorldCenter(_sphericalBody);
            float sphereRadius = GetSphereWorldRadius(_sphericalBody);

            OverlapSphere(sphereCenter, sphereRadius);

            if (_hasPreviousSphereCenter == true
                && _rigidbody != null
                && _rigidbody.isKinematic == false)
            {
                Vector3 displacement = sphereCenter - _previousSphereCenter;
                float castDistance = displacement.magnitude;

                if (castDistance > 0.0001f)
                    AppendSphereCastContacts(_previousSphereCenter, sphereRadius, displacement.normalized, castDistance);
            }

            _previousSphereCenter = sphereCenter;
            _hasPreviousSphereCenter = true;
        }

        private void OverlapSphere(Vector3 center, float radius)
        {
            _contacts.Count = Physics.OverlapSphereNonAlloc(
                center,
                radius,
                _contacts.Items,
                _mask,
                QueryTriggerInteraction.Ignore);
        }

        private void AppendSphereCastContacts(Vector3 origin, float radius, Vector3 direction, float distance)
        {
            int hitCount = Physics.SphereCastNonAlloc(
                origin,
                radius,
                direction,
                _sphereCastHits,
                distance,
                _mask,
                QueryTriggerInteraction.Ignore);

            for (int hitIndex = 0; hitIndex < hitCount; hitIndex++)
            {
                Collider hitCollider = _sphereCastHits[hitIndex].collider;

                if (hitCollider == null)
                    continue;

                if (ContainsCollider(hitCollider) == true)
                    continue;

                if (_contacts.Count >= _contacts.Items.Length)
                    return;

                _contacts.Items[_contacts.Count] = hitCollider;
                _contacts.Count++;
            }
        }

        private void OverlapCapsule()
        {
            _contacts.Count = Physics.OverlapCapsuleNonAlloc(
                _capsuleBody.bounds.min,
                _capsuleBody.bounds.max,
                _capsuleBody.radius,
                _contacts.Items,
                _mask,
                QueryTriggerInteraction.Ignore);
        }

        private bool ContainsCollider(Collider collider)
        {
            for (int contactIndex = 0; contactIndex < _contacts.Count; contactIndex++)
            {
                if (_contacts.Items[contactIndex] == collider)
                    return true;
            }

            return false;
        }

        private static Vector3 GetSphereWorldCenter(SphereCollider sphereCollider)
        {
            return sphereCollider.transform.TransformPoint(sphereCollider.center);
        }

        private static float GetSphereWorldRadius(SphereCollider sphereCollider)
        {
            Vector3 lossyScale = sphereCollider.transform.lossyScale;
            float maxAxisScale = Mathf.Max(lossyScale.x, Mathf.Max(lossyScale.y, lossyScale.z));
            return sphereCollider.radius * maxAxisScale;
        }

        private void RemoveSelfFromContacts(Collider selfCollider)
        {
            int indexToRemove = -1;

            for (int contactIndex = 0; contactIndex < _contacts.Count; contactIndex++)
            {
                if (_contacts.Items[contactIndex] == selfCollider)
                {
                    indexToRemove = contactIndex;
                    break;
                }
            }

            if (indexToRemove < 0)
                return;

            for (int contactIndex = indexToRemove; contactIndex < _contacts.Count - 1; contactIndex++)
                _contacts.Items[contactIndex] = _contacts.Items[contactIndex + 1];

            _contacts.Count--;
        }
    }
}
