using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Meta.NewPowerups
{
    [CreateAssetMenu(menuName = "Configs/Gameplay/Abilities/NewPermanentDamageFirstEnemiesConfigConfig", fileName = "PermanentDamageFirstEnemiesConfig")]
    public class PermanentDamageFirstEnemiesConfig : PowerupConfig
    {
        [field: SerializeField,Range(0f, 1f)] public float PercentOfWaveEnemiesDamaged  { get; private set; } = 0.5f;
        [field: SerializeField,Range(0f, 1f)] public float PercentOfEnemyLifeLost { get; private set; } = 0.5f;
        
        public override int MaxLevel => 1;
    }
}