using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Meta.NewPowerups
{
    [CreateAssetMenu(menuName = "Configs/Gameplay/Abilities/NewPermanentDamageFirstEnemiesConfigConfig", fileName = "PermanentDamageFirstEnemiesConfig")]
    public class PermanentDamageFirstEnemiesConfig : PowerupConfig
    {
        [field: SerializeField] public int DamageableEnemiesCount { get; private set; } = 3;
        [field: SerializeField,Range(0f, 1f)] public float PercentOfEnemyLifeLost { get; private set; } = 0.25f;
        
        public override int MaxLevel => 1;
    }
}