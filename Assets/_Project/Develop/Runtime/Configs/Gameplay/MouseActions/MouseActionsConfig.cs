using UnityEngine;

namespace _Project.Develop.Runtime.Configs.Gameplay.MouseActions
{
    [CreateAssetMenu(menuName = "Configs/Gameplay/MouseActions/NewMouseActionsConfig", fileName = "MouseActionsConfig")]
    public class MouseActionsConfig : ScriptableObject
    {
        [field: SerializeField, Min(0)] public float TowerExplosionDamage { get; private set; } = 100;
        [field: SerializeField, Min(0)] public float TowerExplosionRadius { get; private set; } = 5;

        [field: SerializeField, Min(0)] public float MouseRaycastDistance { get; private set; } = 1000;
    }
}