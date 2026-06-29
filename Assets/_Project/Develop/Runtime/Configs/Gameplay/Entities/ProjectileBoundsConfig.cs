using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Entities
{
    [CreateAssetMenu(menuName = "Configs/Gameplay/Entities/NewProjectileBoundsConfig", fileName = "ProjectileBoundsConfig")]
    public class ProjectileBoundsConfig : ScriptableObject
    {
        [field: SerializeField, Range(0f, 1f)] public float ViewportMargin { get; private set; } = 0.05f;
        [field: SerializeField, Min(0f)] public float MaxLifetimeSeconds { get; private set; } = 3.5f;
        [field: SerializeField, Min(0f)] public float ArenaRadiusMargin { get; private set; } = 4f;
        [field: SerializeField, Min(0f)] public float MaxTravelDistanceMargin { get; private set; } = 8f;
        [field: SerializeField, Min(0f)] public float FallbackMaxTravelDistance { get; private set; } = 75f;
    }
}
