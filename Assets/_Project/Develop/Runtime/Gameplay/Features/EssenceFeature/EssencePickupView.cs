using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.EssenceFeature
{
    public class EssencePickupView : MonoBehaviour
    {
        private const float PICKUP_SCALE = 2.4f;
        private const float PICKUP_COLLIDER_RADIUS = 1.2f;
        private const float PICKUP_FLOOR_OFFSET = 1.6f;

        private float _remainingHoverLockSeconds;
        private SphereCollider _sphereCollider;

        public int Amount { get; private set; }

        public bool IsVacuuming { get; private set; }

        public bool CanAcceptHover { get; private set; }

        public void Initialize(int amount, Vector3 worldPosition, float hoverUnlockDelay)
        {
            Amount = amount;
            CanAcceptHover = false;
            _remainingHoverLockSeconds = hoverUnlockDelay;
            transform.localScale = Vector3.one * PICKUP_SCALE;

            worldPosition.y += PICKUP_FLOOR_OFFSET;
            transform.position = worldPosition;

            ConfigureCollider();
        }

        public void TickHoverLock(float deltaTime)
        {
            if (CanAcceptHover)
                return;

            _remainingHoverLockSeconds -= deltaTime;

            if (_remainingHoverLockSeconds > 0f)
                return;

            CanAcceptHover = true;
            _sphereCollider.enabled = true;
        }

        public void StartVacuuming()
        {
            IsVacuuming = true;
        }

        public void MoveTowards(Vector3 targetPosition, float moveSpeed, float deltaTime)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * deltaTime);
        }

        private void ConfigureCollider()
        {
            _sphereCollider = GetComponent<SphereCollider>();

            if (_sphereCollider == null)
                _sphereCollider = gameObject.AddComponent<SphereCollider>();

            _sphereCollider.isTrigger = false;
            _sphereCollider.radius = PICKUP_COLLIDER_RADIUS;
            _sphereCollider.enabled = false;
        }
    }
}
