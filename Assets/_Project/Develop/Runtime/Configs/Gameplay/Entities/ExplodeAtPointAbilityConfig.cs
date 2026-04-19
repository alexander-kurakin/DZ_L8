using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Entities
{
    [CreateAssetMenu(menuName = "Configs/Gameplay/Entities/NewExplodeAtPointAbilityConfig",
        fileName = "ExplodeAtPointAbilityConfig")]
    public class ExplodeAtPointAbilityConfig : EntityConfig
    {
        [field: SerializeField, Min(0)] public float ExplosionDamage { get; private set; } = 100;
        [field: SerializeField, Min(0)] public float ExplosionRadius { get; private set; } = 5;
    }
}