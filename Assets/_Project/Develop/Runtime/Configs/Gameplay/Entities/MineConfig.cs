using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Entities
{
    [CreateAssetMenu(menuName = "Configs/Gameplay/Entities/NewMineConfig", fileName = "MineConfig")]
    public class MineConfig : EntityConfig
    {
        [field: SerializeField] public string PrefabPath { get; private set; } = "Entities/Mine";
        [field: SerializeField, Min(0)] public float MineDamage { get; private set; } = 100;
        [field: SerializeField, Min(0)] public float MineExplosionRadius { get; private set; } = 5;
        [field: SerializeField, Min(0)] public int MineCostInGold { get; private set; } = 50;
    }
}