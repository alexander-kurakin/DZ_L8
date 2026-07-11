using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Throw
{
    [CreateAssetMenu(menuName = "Configs/Gameplay/Throw/NewThrowChargeConfig", fileName = "ThrowChargeConfig")]
    public class ThrowChargeConfig : ScriptableObject
    {
        [field: SerializeField, Min(0.01f)] public float MaxChargeTimeSeconds { get; private set; } = 1.5f;
        [field: SerializeField, Min(0f)] public float MinThrowPower { get; private set; } = 0f;
        [field: SerializeField, Min(0f)] public float MaxThrowPower { get; private set; } = 10f;
        [field: SerializeField, Min(0f)] public float MinThrowSpeed { get; private set; } = 10f;
        [field: SerializeField, Min(0f)] public float MaxThrowSpeed { get; private set; } = 25f;
        [field: SerializeField] public AnimationCurve SpeedCurve { get; private set; } = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        [field: SerializeField, Range(-89f, 89f)] public float WeakThrowMinElevationDegrees { get; private set; } = 18f;
        [field: SerializeField, Range(-89f, 89f)] public float FullThrowMinElevationDegrees { get; private set; } = -30f;
        [field: SerializeField, Range(-89f, 89f)] public float FullThrowMaxElevationDegrees { get; private set; } = 35f;
        [field: SerializeField] public AnimationCurve PitchBiasCurve { get; private set; } = AnimationCurve.Linear(0f, 1f, 1f, 0.65f);
        [field: SerializeField, Range(0f, 1f)] public float DirectionStabilizationPowerRatio { get; private set; } = 0.7f;
        [field: SerializeField, Min(0f)] public float ProjectileRespawnCooldownSeconds { get; private set; } = 1f;
        [field: SerializeField, Min(0f)] public float PostImpactAimLockSeconds { get; private set; } = 0.5f;
        [field: SerializeField, Min(1)] public int MaxProjectilesInScene { get; private set; } = 5;
        [field: SerializeField, Min(0f)] public float ProjectileMaxDistanceFromOwner { get; private set; } = 25f;
        [field: SerializeField, Min(8)] public int TrajectoryPreviewMaxPoints { get; private set; } = 64;
        [field: SerializeField] public AnimationCurve ChargeCurve { get; private set; } = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        public float EvaluateChargeRatio(float chargeRatio)
        {
            if (ChargeCurve == null || ChargeCurve.length == 0)
                return chargeRatio;

            return ChargeCurve.Evaluate(chargeRatio);
        }

        public float EvaluateThrowSpeed(float throwPower)
        {
            if (MaxThrowPower <= MinThrowPower)
                return MaxThrowSpeed;

            float powerRatio = EvaluateThrowPowerRatio(throwPower);
            float speedRatio = EvaluateSpeedRatio(powerRatio);
            return Mathf.Lerp(MinThrowSpeed, MaxThrowSpeed, speedRatio);
        }

        public float EvaluateThrowPowerRatio(float throwPower)
        {
            if (MaxThrowPower <= MinThrowPower)
                return 1f;

            return Mathf.InverseLerp(MinThrowPower, MaxThrowPower, throwPower);
        }

        public Vector3 ResolveThrowDirection(Vector3 cameraDirection, float throwPower)
        {
            if (cameraDirection.sqrMagnitude <= 0f)
                return cameraDirection;

            cameraDirection.Normalize();

            float powerRatio = EvaluateThrowPowerRatio(throwPower);
            float directionPowerRatio = Mathf.Min(powerRatio, DirectionStabilizationPowerRatio);
            float elevationBlend = EvaluatePitchBiasMultiplier(directionPowerRatio);
            Vector3 horizontalDirection = GetHorizontalDirection(cameraDirection);
            float cameraElevationDegrees = GetElevationDegrees(cameraDirection);
            float targetElevationDegrees = Mathf.Lerp(cameraElevationDegrees, WeakThrowMinElevationDegrees, elevationBlend);
            targetElevationDegrees = Mathf.Clamp(
                targetElevationDegrees,
                FullThrowMinElevationDegrees,
                FullThrowMaxElevationDegrees);

            return BuildDirectionFromElevation(horizontalDirection, targetElevationDegrees);
        }

        private static float GetElevationDegrees(Vector3 direction)
        {
            Vector3 horizontalDirection = new Vector3(direction.x, 0f, direction.z);
            return Mathf.Atan2(direction.y, horizontalDirection.magnitude) * Mathf.Rad2Deg;
        }

        private static Vector3 GetHorizontalDirection(Vector3 direction)
        {
            Vector3 horizontalDirection = new Vector3(direction.x, 0f, direction.z);

            if (horizontalDirection.sqrMagnitude <= 0.0001f)
                return Vector3.forward;

            return horizontalDirection.normalized;
        }

        private static Vector3 BuildDirectionFromElevation(Vector3 horizontalDirection, float elevationDegrees)
        {
            float elevationRadians = elevationDegrees * Mathf.Deg2Rad;
            float cosElevation = Mathf.Cos(elevationRadians);
            float sinElevation = Mathf.Sin(elevationRadians);

            return new Vector3(
                horizontalDirection.x * cosElevation,
                sinElevation,
                horizontalDirection.z * cosElevation);
        }

        private float EvaluatePitchBiasMultiplier(float powerRatio)
        {
            if (PitchBiasCurve == null || PitchBiasCurve.length == 0)
                return 1f - powerRatio;

            return PitchBiasCurve.Evaluate(powerRatio);
        }

        private float EvaluateSpeedRatio(float powerRatio)
        {
            if (SpeedCurve == null || SpeedCurve.length == 0)
                return powerRatio;

            return Mathf.Clamp01(SpeedCurve.Evaluate(powerRatio));
        }
    }
}
