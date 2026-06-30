using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.TowerWalker
{
    public static class BrotherStoneThrowAimUtility
    {
        private const float MIN_DIRECTION_SQR_MAGNITUDE = 0.0001f;
        private const float FALLBACK_AIM_HEIGHT = 1.5f;
        private const int INTERCEPT_ITERATION_COUNT = 2;

        public static Vector3 GetEnemyAimPoint(Entity enemy)
        {
            if (enemy.TryGetRigidbody(out Rigidbody rigidbody))
                return rigidbody.worldCenterOfMass;

            if (enemy.TryGetBodyCollider(out CapsuleCollider bodyCollider))
                return bodyCollider.bounds.center;

            if (enemy.TryGetTransform(out Transform enemyTransform))
                return enemyTransform.position + Vector3.up * FALLBACK_AIM_HEIGHT;

            return Vector3.zero;
        }

        public static Vector3 GetEnemyVelocity(Entity enemy)
        {
            if (enemy.TryGetRigidbody(out Rigidbody rigidbody))
            {
                if (rigidbody.velocity.sqrMagnitude > MIN_DIRECTION_SQR_MAGNITUDE)
                    return rigidbody.velocity;
            }

            if (enemy.TryGetMoveDirection(out ReactiveVariable<Vector3> moveDirection) == false)
                return Vector3.zero;

            if (enemy.TryGetMoveSpeed(out ReactiveVariable<float> moveSpeed) == false)
                return Vector3.zero;

            return moveDirection.Value * moveSpeed.Value;
        }

        public static Vector3 GetPredictedAimPoint(Vector3 spawnWorldPosition, Entity target, float projectileSpeed)
        {
            Vector3 aimPoint = GetEnemyAimPoint(target);
            Vector3 targetVelocity = GetEnemyVelocity(target);

            if (projectileSpeed <= 0f)
                return aimPoint;

            if (targetVelocity.sqrMagnitude <= MIN_DIRECTION_SQR_MAGNITUDE)
                return aimPoint;

            float flightTime = (aimPoint - spawnWorldPosition).magnitude / projectileSpeed;

            for (int iteration = 0; iteration < INTERCEPT_ITERATION_COUNT; iteration++)
            {
                aimPoint = GetEnemyAimPoint(target) + targetVelocity * flightTime;
                flightTime = (aimPoint - spawnWorldPosition).magnitude / projectileSpeed;
            }

            return aimPoint;
        }
    }
}
