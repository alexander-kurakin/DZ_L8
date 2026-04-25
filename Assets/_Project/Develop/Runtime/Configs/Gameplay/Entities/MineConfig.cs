using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Entities
{
    [CreateAssetMenu(menuName = "Configs/Gameplay/Entities/NewMineConfig", fileName = "MineConfig")]
    public class MineConfig : PurchasableEntityConfig
    {
        [field: SerializeField, Min(0)] public float Damage { get; private set; } = 100;
        [field: SerializeField, Min(0)] public float ExplosionRadius { get; private set; } = 5;
    }
}