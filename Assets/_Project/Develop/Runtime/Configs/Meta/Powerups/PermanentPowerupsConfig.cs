using _Project.Develop.Runtime.Meta.Features.Powerups;
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

        [field: SerializeField] public int TowerExtraHealthPriceDiamonds { get; private set; } = 150;
        [field: SerializeField] public int FirstEnemiesDebuffPriceDiamonds { get; private set; } = 250;
        [field: SerializeField] public int PowerfulClickPriceDiamonds { get; private set; } = 400;

        public int GetDiamondPriceBy(PowerupType type)
        {
            int diamondPrice = 0;

            switch (type)
            {
                case PowerupType.HealExtra:
                    diamondPrice = TowerExtraHealthPriceDiamonds;
                    break;
                case PowerupType.DamageFirstEnemies:
                    diamondPrice = FirstEnemiesDebuffPriceDiamonds;
                    break;
                case PowerupType.IncreaseClickDamage:
                    diamondPrice = PowerfulClickPriceDiamonds;
                    break;
            }
            
            return diamondPrice;
        }
    }
}