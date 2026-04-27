using UnityEngine;

namespace _Project.Develop.Runtime.Configs.Meta.Powerups
{
    [CreateAssetMenu(menuName = "Configs/Meta/Powerups/NewPermanentPowerupsConfig", fileName = "PermanentPowerupsConfig")]
    public class PermanentPowerupsConfig : ScriptableObject
    {
        [field: SerializeField,Range(0f, 1f)] public float TowerExtraHealthPerc { get; private set; } = 0.15f;
        [field: SerializeField] public int FirstEnemiesInWaveCount { get; private set; } = 5;
        [field: SerializeField, Range(0f, 1f)] public float FirstEnemiesInWaveDebuffPerc { get; private set; } = 0.25f;
        [field: SerializeField] public float ExplosionDamageIncreasedMult { get; private set; } = 1.5f;
        
        
    }
}