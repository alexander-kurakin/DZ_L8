using DG.Tweening;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.EssenceFeature
{
    public class EssencePickupView : MonoBehaviour
    {
        private const float PICKUP_SCALE = 2.4f;
        private const float PICKUP_COLLIDER_RADIUS = 1.2f;
        private const float PICKUP_FLOOR_OFFSET = 1.6f;
        private const float VACUUM_SHRINK_SCALE_FACTOR = 0.4f;
        private const float VACUUM_SHRINK_DURATION_SECONDS = 0.35f;
        private const float HOVER_READY_POP_SCALE_FACTOR = 1.35f;
        private const float HOVER_READY_POP_DURATION_SECONDS = 0.2f;

        private static readonly Color HOVER_READY_COLOR = new Color(0.65f, 1f, 1f, 1f);

        private float _remainingHoverLockSeconds;
        private SphereCollider _sphereCollider;
        private Renderer _pickupRenderer;
        private Material _pickupMaterial;
        private Color _baseColor;
        private Tween _vacuumScaleTween;
        private Tween _colorTween;
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

            _pickupRenderer = GetComponent<Renderer>();
            _pickupMaterial = _pickupRenderer.material;
            _baseColor = _pickupRenderer.material.color;

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

            float popScale = PICKUP_SCALE * HOVER_READY_POP_SCALE_FACTOR;
            transform.DOScale(Vector3.one * popScale, HOVER_READY_POP_DURATION_SECONDS).SetEase(Ease.OutBack);

            if (_pickupMaterial != null)
            {
                _colorTween?.Kill();
                _colorTween = _pickupMaterial.DOColor(HOVER_READY_COLOR, HOVER_READY_POP_DURATION_SECONDS);
            }
        }

        public void StartVacuuming()
        {
            if (IsVacuuming)
                return;

            IsVacuuming = true;

            float shrunkScale = PICKUP_SCALE * VACUUM_SHRINK_SCALE_FACTOR;
            _vacuumScaleTween?.Kill();
            _vacuumScaleTween = transform
                .DOScale(Vector3.one * shrunkScale, VACUUM_SHRINK_DURATION_SECONDS)
                .SetEase(Ease.InBack);
        }

        public Vector3 GetWorldPosition()
        {
            return transform.position;
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

            _sphereCollider.isTrigger = true;
            _sphereCollider.radius = PICKUP_COLLIDER_RADIUS;
            _sphereCollider.enabled = false;
        }

        private void OnDestroy()
        {
            _vacuumScaleTween?.Kill();
            _colorTween?.Kill();
        }
    }
}
