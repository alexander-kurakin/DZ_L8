using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Entities
{
    [CreateAssetMenu(menuName = "Configs/Gameplay/Entities/NewDragonEnrageConfig", fileName = "DragonEnrageConfig")]
    public class DragonEnrageConfig : ScriptableObject
    {
        [field: SerializeField, Min(0f)] public float OutgoingDamageBonusPerStack { get; private set; } = 0.5f;
        [field: SerializeField, Min(0f)] public float EnrageEffectScalePerStack { get; private set; } = 0.08f;
        [field: SerializeField, Min(0f)] public float EnragePunchScale { get; private set; } = 1.15f;
        [field: SerializeField, Min(0f)] public float EnragePunchDurationSeconds { get; private set; } = 0.2f;
        [field: SerializeField, Min(0f)] public float EnragePunchScalePerStack { get; private set; } = 0.04f;
        [field: SerializeField, Min(0)] public int EnragePunchMaxStacksForScale { get; private set; } = 4;
    }
}
