using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Entities
{
    [CreateAssetMenu(
        menuName = "Configs/Gameplay/Entities/NewTowerBrotherStoneThrowConfig",
        fileName = "TowerBrotherStoneThrowConfig")]
    public class TowerBrotherStoneThrowConfig : ScriptableObject
    {
        [field: SerializeField, Min(0f)] public float ThrowIntervalSeconds { get; private set; } = 3f;
        [field: SerializeField, Min(0f)] public float DamagePerThrow { get; private set; } = 95f;
        [field: SerializeField, Min(0f)] public float ThrowAnimationDurationSeconds { get; private set; } = 0.65f;
        [field: SerializeField, Min(0f)] public float ProjectileSpeed { get; private set; } = 28f;
        [field: SerializeField, Min(0f)] public float ProjectileScale { get; private set; } = 0.35f;
        [field: SerializeField, Min(0f)] public float ProjectileSpawnHeight { get; private set; } = 1.5f;
        [field: SerializeField, Min(0f)] public float ArcMaxHeight { get; private set; } = 5f;
        [field: SerializeField] public AnimationCurve ArcHeightCurve { get; private set; } = CreateDefaultArcHeightCurve();

        private static AnimationCurve CreateDefaultArcHeightCurve()
        {
            return new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.5f, 1f),
                new Keyframe(1f, 0f));
        }
    }
}
