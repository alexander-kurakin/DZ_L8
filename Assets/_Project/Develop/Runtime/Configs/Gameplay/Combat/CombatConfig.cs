using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Combat
{
    [CreateAssetMenu(menuName = "Configs/Gameplay/Combat/NewCombatConfig", fileName = "CombatConfig")]
    public class CombatConfig : ScriptableObject
    {
        [field: SerializeField, Min(0)] public float EnemyMoveSpeedScale { get; private set; } = 1f;
        [field: SerializeField, Min(0)] public float HeroMaxHealth { get; private set; } = 100f;
    }
}
