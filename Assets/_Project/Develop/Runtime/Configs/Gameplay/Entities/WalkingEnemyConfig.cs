using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Entities
{
    [CreateAssetMenu(menuName = "Configs/Gameplay/Entities/NewWalkingEnemyConfig", fileName = "WalkingEnemyConfig")]
    public class WalkingEnemyConfig : EntityConfig
    {
        [field: SerializeField, Min(0)] public float MoveSpeed { get; private set; } = 9;
        [field: SerializeField, Min(0)] public float RotationSpeed { get; private set; } = 900;
        [field: SerializeField, Min(0)] public float MaxHealth { get; private set; } = 100;
        [field: SerializeField, Min(0)] public float DeathProcessTime { get; private set; } = 1;
        [field: SerializeField, Min(0)] public float DistanceToTargetGoal { get; private set; } = 5;
        [field: SerializeField, Min(0)] public float ExplosionDamage { get; private set; } = 50;
        [field: SerializeField, Min(0)] public float SpawnProcessTime { get; private set; } = 2;
    }
}