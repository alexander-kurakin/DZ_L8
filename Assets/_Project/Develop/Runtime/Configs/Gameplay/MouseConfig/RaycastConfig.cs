using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.MouseConfig
{
    [CreateAssetMenu(menuName = "Configs/Gameplay/MouseConfig/RaycastConfig", fileName = "RaycastConfig")]
    public class RaycastConfig : ScriptableObject
    {
        [field: SerializeField, Min(0)] public float MouseRaycastDistance { get; private set; } = 1000;
    }
}