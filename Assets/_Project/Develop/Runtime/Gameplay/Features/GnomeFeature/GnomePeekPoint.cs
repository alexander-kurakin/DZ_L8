using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.GnomeFeature
{
    public class GnomePeekPoint : MonoBehaviour
    {
        private const float PEEK_POINT_MARKER_RADIUS = 0.08f;

        [SerializeField] private GnomeCoverAnchor _coverAnchor;
        [SerializeField] private float _peekOffset = 0.55f;
        [SerializeField] private bool _isVerticalLayout;

        private Entity _occupyingEntity;

        public bool IsOccupied => _occupyingEntity != null;
        public Entity OccupyingEntity => _occupyingEntity;
        public GnomeCoverAnchor CoverAnchor => _coverAnchor;
        public float PeekOffset => _peekOffset;
        public bool IsVerticalLayout => _isVerticalLayout;
        public Vector3 PeekDirection => transform.forward;
        public Vector3 HiddenPosition => transform.position;
        public Quaternion HiddenRotation => transform.rotation;

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

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0f, 1f, 1f, 0.9f);
            Gizmos.DrawWireSphere(transform.position, PEEK_POINT_MARKER_RADIUS);

            Gizmos.color = new Color(0f, 1f, 0.4f, 0.9f);
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * _peekOffset);
        }
    }
}
