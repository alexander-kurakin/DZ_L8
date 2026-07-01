using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Meta.NewPowerups
{
    [CreateAssetMenu(menuName = "Configs/Gameplay/Abilities/NewPermanentStrongerBuffConfig", fileName = "PermanentStrongerBuffConfig")]
    public class PermanentStrongerBuffConfig : PowerupConfig
    {
        [field: SerializeField, Min(1f)] public float BuffDamageMultiplier { get; private set; } = 2f;
        [field: SerializeField, Min(1f)] public float BuffEssenceCostMultiplier { get; private set; } = 2f;

        public override int MaxLevel => 1;
    }
}
