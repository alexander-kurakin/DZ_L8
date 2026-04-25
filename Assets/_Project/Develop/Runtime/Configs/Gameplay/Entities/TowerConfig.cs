using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Entities
{
    [CreateAssetMenu(menuName = "Configs/Gameplay/Entities/NewTowerConfig", fileName = "TowerConfig")]
    public class TowerConfig : EntityConfig
    {
        [field: SerializeField, Min(0)] public float DeathProcessTime { get; private set; } = 1;
        [field: SerializeField] public Vector3 StartPosition { get; private set; } = Vector3.zero;
    }
}
