using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.TowerWalker
{
    public static class BrotherStoneArcUtility
    {
        public static Vector3 EvaluateArcPosition(
            Vector3 startPosition,
            Vector3 targetPosition,
            float normalizedProgress,
            AnimationCurve heightCurve,
            float arcMaxHeight)
        {
            Vector3 position = Vector3.Lerp(startPosition, targetPosition, normalizedProgress);

            if (heightCurve == null || arcMaxHeight <= 0f)
                return position;

            float heightOffset = heightCurve.Evaluate(normalizedProgress) * arcMaxHeight;
            position.y += heightOffset;

            return position;
        }
    }
}
