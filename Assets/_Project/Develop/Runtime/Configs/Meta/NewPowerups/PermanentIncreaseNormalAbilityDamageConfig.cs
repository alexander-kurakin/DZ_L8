using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Meta.NewPowerups
{
    [CreateAssetMenu(menuName = "Configs/Gameplay/Abilities/NewPermanentIncreaseNormalAbilityDamageConfig", fileName = "PermanentIncreaseNormalAbilityDamageConfig")]
    public class PermanentIncreaseNormalAbilityDamageConfig : PowerupConfig
    {
        [field: SerializeField] public float DamageMultiplier { get; private set; } = 1.5f;
        
        public override int MaxLevel => 1;
    }
}