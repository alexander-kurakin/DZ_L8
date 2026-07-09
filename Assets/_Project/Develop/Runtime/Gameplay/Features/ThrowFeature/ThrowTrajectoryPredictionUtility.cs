using System.Collections.Generic;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.ThrowFeature
{
    public static class ThrowTrajectoryPredictionUtility
    {
        private const float SIMULATION_TIME_STEP = 0.05f;
        private const float MAX_SIMULATION_TIME_SECONDS = 8f;
        private const float PROJECTILE_RADIUS = 0.175f;

        public static void Predict(
            List<Vector3> trajectoryPoints,
            Vector3 origin,
            Vector3 direction,
            float speed,
            Transform ignoreTransform,
            int maxPoints)
        {
            trajectoryPoints.Clear();

            if (speed <= 0f)
                return;

            if (direction.sqrMagnitude <= 0f)
                return;

            Vector3 velocity = direction.normalized * speed;
            Vector3 position = origin;
            float elapsedTime = 0f;

            trajectoryPoints.Add(origin);

            while (elapsedTime < MAX_SIMULATION_TIME_SECONDS && trajectoryPoints.Count < maxPoints)
            {
                velocity += Physics.gravity * SIMULATION_TIME_STEP;
                Vector3 nextPosition = position + velocity * SIMULATION_TIME_STEP;
                Vector3 segment = nextPosition - position;
                float segmentLength = segment.magnitude;

                if (segmentLength > 0f)
                {
                    if (TryGetCollisionPoint(position, segment, segmentLength, ignoreTransform, out Vector3 collisionPoint) == true)
                    {
                        trajectoryPoints.Add(collisionPoint);
                        return;
                    }
                }

                position = nextPosition;
                elapsedTime += SIMULATION_TIME_STEP;
                trajectoryPoints.Add(position);
            }
        }

        private static bool TryGetCollisionPoint(
            Vector3 origin,
            Vector3 segment,
            float segmentLength,
            Transform ignoreTransform,
            out Vector3 collisionPoint)
        {
            collisionPoint = default;

            if (Physics.SphereCast(
                    origin,
                    PROJECTILE_RADIUS,
                    segment.normalized,
                    out RaycastHit hit,
                    segmentLength) == false)
                return false;

            if (ignoreTransform != null && hit.transform.IsChildOf(ignoreTransform) == true)
                return false;

            collisionPoint = hit.point;
            return true;
        }
    }
}
