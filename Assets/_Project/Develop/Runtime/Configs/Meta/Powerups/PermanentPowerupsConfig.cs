using UnityEngine;

namespace _Project.Develop.Runtime.Configs.Meta.Powerups
{
    [CreateAssetMenu(menuName = "Configs/Meta/Powerups/NewPermanentPowerupsConfig", fileName = "PermanentPowerupsConfig")]
    public class PermanentPowerupsConfig : ScriptableObject
    {
        [field: SerializeField] public float TowerCurrentHealthMult { get; private set; } = 0.15f;
        [field: SerializeField] public int FirstEnemiesInWaveCount { get; private set; } = 5;
        [field: SerializeField] public float FirstEnemiesInWaveDamageMult { get; private set; } = 1.25f;
        [field: SerializeField] public float ExplosionDamageIncreasedMult { get; private set; } = 1.3f;
    }
}