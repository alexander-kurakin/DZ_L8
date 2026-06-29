using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Juice
{
    [CreateAssetMenu(menuName = "Configs/Gameplay/Juice/NewGameplayVfxConfig", fileName = "GameplayVfxConfig")]
    public class GameplayVfxConfig : ScriptableObject
    {
        [field: SerializeField, Min(0f)] public float DefaultEffectLifetimeSeconds { get; private set; } = 3f;
        [field: SerializeField, Min(0f)] public float EffectLifetimePaddingSeconds { get; private set; } = 0.35f;
        [field: SerializeField, Min(0f)] public float SmallScreenShakeDurationSeconds { get; private set; } = 0.12f;
        [field: SerializeField, Min(0f)] public float SmallScreenShakeStrength { get; private set; } = 0.12f;
        [field: SerializeField, Min(0f)] public float MediumScreenShakeDurationSeconds { get; private set; } = 0.22f;
        [field: SerializeField, Min(0f)] public float MediumScreenShakeStrength { get; private set; } = 0.28f;
        [field: SerializeField, Min(1)] public int ScreenShakeVibrato { get; private set; } = 18;
        [field: SerializeField, Range(0f, 90f)] public float ScreenShakeRandomness { get; private set; } = 45f;
    }
}
