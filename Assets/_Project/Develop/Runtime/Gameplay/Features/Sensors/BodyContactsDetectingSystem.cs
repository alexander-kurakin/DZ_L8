using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Utilities;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Sensors
{
    public class BodyContactsDetectingSystem : IInitializableSystem, IUpdatableSystem
    {
        private Buffer<Collider> _contacts;
        private LayerMask _mask;
        private CapsuleCollider _capsuleBody;
        private SphereCollider _sphericalBody;
        private ColliderType _colliderType;

        public BodyContactsDetectingSystem(ColliderType colliderType)
        {
            _colliderType = colliderType;
        }

        public void OnInit(Entity entity)
        {
            _contacts = entity.ContactCollidersBuffer;
            _mask = entity.ContactsDetectingMask;

            switch (_colliderType)
            {
                case ColliderType.Capsule:
                    _capsuleBody = entity.BodyCollider;
                    break;
                case ColliderType.Sphere:
                    _sphericalBody = entity.MineCollider;
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
                    OverlapSphere();
                    RemoveSelfFromContacts(_sphericalBody);
                    break;
            }
        }

        private void OverlapSphere()
        {
            _contacts.Count = Physics.OverlapSphereNonAlloc(
                _sphericalBody.transform.position,
                _sphericalBody.radius,
                _contacts.Items,
                _mask,
                QueryTriggerInteraction.Ignore);
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

        private void RemoveSelfFromContacts(Collider selfCollider)
        {
            int indexToRemove = -1;

            for (int i = 0; i < _contacts.Count; i++)
            {
                if (_contacts.Items[i] == selfCollider)
                {
                    indexToRemove = i;
                    break;
                }
            }

            if (indexToRemove >= 0)
            {
                for (int i = indexToRemove; i < _contacts.Count - 1; i++)
                {
                    _contacts.Items[i] = _contacts.Items[i + 1];
                }

                _contacts.Count--;
            }
        }
    }
}
