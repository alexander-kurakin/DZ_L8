using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Entities
{
    [CreateAssetMenu(menuName = "Configs/Gameplay/Entities/NewExplodeAtPointAbilityConfig",
        fileName = "ExplodeAtPointAbilityConfig")]
    public class ExplodeAtPointAbilityConfig : EntityConfig
    {
        [field: SerializeField, Min(0)] public float CooldownSeconds { get; private set; } = 5f;
        [field: SerializeField, Range(0f, 1f)] public float CatMaxHealthDamageFraction { get; private set; } = 0.5f;
    }
}