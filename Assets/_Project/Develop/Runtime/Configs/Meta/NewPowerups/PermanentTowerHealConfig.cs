using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Meta.NewPowerups
{
    [CreateAssetMenu(menuName = "Configs/Gameplay/Abilities/NewPermanentTowerHealConfig", fileName = "PermanentTowerHealConfig")]
    public class PermanentTowerHealConfig : PowerupConfig
    {
        [field: SerializeField,Range(0f, 1f)] public float PercentOfTowerLifeHealed { get; private set; } = 0.15f;
        
        public override int MaxLevel => 1;
    }
}