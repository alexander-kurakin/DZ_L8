using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Entities
{
    [CreateAssetMenu(menuName = "Configs/Gameplay/Entities/NewTurretConfig", fileName = "TurretConfig")]
    public class TurretConfig : PurchasableEntityConfig
    {
        [field: SerializeField, Min(0)] public float Damage { get; private set; } = 15;
        [field: SerializeField, Min(0)] public float RotationSpeed { get; private set; } = 300;
        
        [field: SerializeField, Min(0)] public float AttackProcessTime { get; private set; } = 1;
        [field: SerializeField, Min(0)] public float AttackDelayTime { get; private set; } = 0.3f;
        [field: SerializeField, Min(0)] public float AttackCooldown { get; private set; } = 1;
    }
}