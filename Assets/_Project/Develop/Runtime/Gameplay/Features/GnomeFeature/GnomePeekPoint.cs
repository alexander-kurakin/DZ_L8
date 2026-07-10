using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.GnomeFeature
{
    public class GnomePeekPoint : MonoBehaviour
    {
        private const float PEEK_POINT_MARKER_RADIUS = 0.08f;
        private const float LEAN_AXIS_MIN_SQR_MAGNITUDE = 0.0001f;
        private const float LEAN_GIZMO_LENGTH = 1.5f;

        [SerializeField] private GnomeCoverAnchor _coverAnchor;
        [SerializeField] private float _peekOffset = 0.55f;
        [SerializeField] private float _peekLeanAngle = 25f;
        [SerializeField] private bool _isVerticalLayout;

        private Entity _occupyingEntity;

        public bool IsOccupied => _occupyingEntity != null;
        public Entity OccupyingEntity => _occupyingEntity;
        public GnomeCoverAnchor CoverAnchor => _coverAnchor;
        public float PeekOffset => _peekOffset;
        public float PeekLeanAngle => _peekLeanAngle;
        public bool IsVerticalLayout => _isVerticalLayout;
        public Vector3 PeekDirection => transform.forward;
        public Vector3 HiddenPosition => transform.position;

        public Quaternion HiddenRotation()
        {
            if (!_isVerticalLayout)
                return transform.rotation;
            else
                return transform.rotation * Quaternion.Euler(90f, 0f, 0f);
        }

        public bool TryOccupy(Entity entity)
        {
            if (entity == null)
                return false;

            if (_occupyingEntity != null)
                return false;

            _occupyingEntity = entity;
            return true;
        }

        public void Release()
        {
            _occupyingEntity = null;
        }

        public static Quaternion GetPeekLeanRotation(Quaternion baseRotation, Vector3 peekDirection, float leanAngle)
        {
            if (Mathf.Abs(leanAngle) <= 0.01f)
                return baseRotation;

            Vector3 normalizedPeekDirection = peekDirection.normalized;
            Vector3 leanAxis = Vector3.Cross(Vector3.up, normalizedPeekDirection);

            if (leanAxis.sqrMagnitude < LEAN_AXIS_MIN_SQR_MAGNITUDE)
                return baseRotation;

            leanAxis.Normalize();
            return Quaternion.AngleAxis(leanAngle, leanAxis) * baseRotation;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0f, 1f, 1f, 0.9f);
            Gizmos.DrawWireSphere(transform.position, PEEK_POINT_MARKER_RADIUS);

            Gizmos.color = new Color(0f, 1f, 0.4f, 0.9f);
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * _peekOffset);

            if (_isVerticalLayout == true || Mathf.Abs(_peekLeanAngle) <= 0.01f)
                return;

            Quaternion leanedRotation = GetPeekLeanRotation(transform.rotation, transform.forward, _peekLeanAngle);
            Vector3 leanedUpDirection = leanedRotation * Vector3.up;

            Gizmos.color = new Color(1f, 0.6f, 0f, 0.9f);
            Gizmos.DrawLine(transform.position, transform.position + leanedUpDirection * LEAN_GIZMO_LENGTH);
        }
    }
}
