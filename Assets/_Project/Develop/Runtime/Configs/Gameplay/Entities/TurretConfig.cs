using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Entities
{
    [CreateAssetMenu(menuName = "Configs/Gameplay/Entities/NewTurretConfig", fileName = "TurretConfig")]
    public class TurretConfig : PurchasableEntityConfig
    {
        [field: SerializeField, Min(0)] public float Damage { get; private set; } = 15;
        [field: SerializeField, Min(0)] public float ActionRadius { get; private set; } = 15;
        [field: SerializeField, Min(0)] public float CooldownTime { get; private set; } = 0.5f;
    }
}