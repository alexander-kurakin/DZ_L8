using Assets._Project.Develop.Runtime.Utilities;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.TowerWalker
{
    public static class TowerWalkBoundsMovementUtility
    {
        private const float CAPSULE_RADIUS_SCALE = 0.95f;
        private const float EDGE_BLOCK_CAST_DISTANCE = 0.45f;

        private static readonly LayerMask TowerWalkBoundsMask = Layers.FenceMask | (1 << 0);

        public static bool IsMovementBlockedByTowerEdge(CapsuleCollider bodyCollider, Vector3 moveDirection)
        {
            if (bodyCollider == null)
                return false;

            if (moveDirection.sqrMagnitude <= 0.0001f)
                return false;

            TryGetWorldCapsule(bodyCollider, out Vector3 bottom, out Vector3 top, out float radius);

            return Physics.CapsuleCast(
                bottom,
                top,
                radius,
                moveDirection.normalized,
                EDGE_BLOCK_CAST_DISTANCE,
                TowerWalkBoundsMask,
                QueryTriggerInteraction.Ignore);
        }

        private static void TryGetWorldCapsule(
            CapsuleCollider bodyCollider,
            out Vector3 bottom,
            out Vector3 top,
            out float radius)
        {
            Transform bodyTransform = bodyCollider.transform;
            Vector3 localCenter = bodyCollider.center;
            float halfHeight = Mathf.Max(bodyCollider.height * 0.5f - bodyCollider.radius, 0f);
            Vector3 localUp = GetCapsuleLocalUp(bodyCollider.direction);
            Vector3 worldCenter = bodyTransform.TransformPoint(localCenter);
            Vector3 worldUp = bodyTransform.TransformDirection(localUp) * halfHeight;

            bottom = worldCenter - worldUp;
            top = worldCenter + worldUp;
            radius = bodyCollider.radius * CAPSULE_RADIUS_SCALE;
        }

        private static Vector3 GetCapsuleLocalUp(int directionAxis)
        {
            if (directionAxis == 0)
                return Vector3.right;

            if (directionAxis == 2)
                return Vector3.forward;

            return Vector3.up;
        }
    }
}
