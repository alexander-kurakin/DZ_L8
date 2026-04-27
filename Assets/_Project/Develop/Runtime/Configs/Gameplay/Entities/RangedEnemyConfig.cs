using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Entities
{
    [CreateAssetMenu(menuName = "Configs/Gameplay/Entities/NewRangedEnemyConfig", fileName = "RangedEnemyConfig")]
    public class RangedEnemyConfig : EntityConfig
    {
        [field: SerializeField, Min(0)] public float MoveSpeed { get; private set; } = 9;
        [field: SerializeField, Min(0)] public float RotationSpeed { get; private set; } = 900;
        [field: SerializeField, Min(0)] public float MaxHealth { get; private set; } = 100;
        [field: SerializeField, Min(0)] public float DeathProcessTime { get; private set; } = 1;
        [field: SerializeField, Min(0)] public float AttackRange { get; private set; } = 15;
        [field: SerializeField, Min(0)] public float AttackProcessTime { get; private set; } = 1;
        [field: SerializeField, Min(0)] public float AttackDelayTime { get; private set; } = 0.3f;
        [field: SerializeField, Min(0)] public float AttackCooldown { get; private set; } = 2;
        [field: SerializeField, Min(0)] public float InstantDamage { get; private set; } = 50;
        [field: SerializeField, Min(0)] public float SpawnProcessTime { get; private set; } = 2;
    }
}