using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Camera
{
    [CreateAssetMenu(menuName = "Configs/Gameplay/Camera/NewHeroCameraConfig", fileName = "HeroCameraConfig")]
    public class HeroCameraConfig : ScriptableObject
    {
        [field: SerializeField, Min(0.01f)] public float ModeTransitionSeconds { get; private set; } = 0.35f;
        [field: SerializeField, Min(1f)] public float FirstPersonFieldOfView { get; private set; } = 70f;
    }
}
