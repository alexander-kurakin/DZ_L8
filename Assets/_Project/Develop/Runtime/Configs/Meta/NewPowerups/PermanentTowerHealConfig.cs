using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Meta.NewPowerups
{
    [CreateAssetMenu(menuName = "Configs/Gameplay/Abilities/NewPermanentLuckyRestoreConfig", fileName = "PermanentLuckyRestoreConfig")]
    public class PermanentLuckyRestoreConfig : PowerupConfig
    {
        [field: SerializeField, Range(0f, 1f)] public float MinMissingIntegrityRestoreFraction { get; private set; } = 0.33f;
        [field: SerializeField, Range(0f, 1f)] public float MaxMissingIntegrityRestoreFraction { get; private set; } = 1f;

        public override int MaxLevel => 1;
    }
}
