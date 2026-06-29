using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Juice
{
    [CreateAssetMenu(menuName = "Configs/Gameplay/Juice/NewGameplayVfxConfig", fileName = "GameplayVfxConfig")]
    public class GameplayVfxConfig : ScriptableObject
    {
        [field: SerializeField, Min(0f)] public float DefaultEffectLifetimeSeconds { get; private set; } = 3f;
        [field: SerializeField, Min(0f)] public float EffectLifetimePaddingSeconds { get; private set; } = 0.35f;
    }
}
