using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Entities
{
    [CreateAssetMenu(menuName = "Configs/Gameplay/Entities/NewToxicAreaConfig", fileName = "ToxicAreaConfig")]
    public class ToxicAreaConfig : PurchasableEntityConfig
    {
        [field: SerializeField, Min(0)] public float DamagePerTick { get; private set; } = 10;
        [field: SerializeField, Min(0)] public float DamageInterval { get; private set; } = 1;
        [field: SerializeField, Range(0f, 1f)] public float SlowMoveSpeedFraction { get; private set; } = 0.33f;
        [field: SerializeField] public GameObject SlowAuraPrefab { get; private set; }
        [field: SerializeField, Min(0)] public float SlowAuraBaseScale { get; private set; } = 1.2f;
        [field: SerializeField] public Vector3 SlowAuraLocalPositionOffset { get; private set; } = new Vector3(0f, -0.75f, 0f);
        [field: SerializeField] public Vector3 SlowAuraLocalScaleMultiplier { get; private set; } = new Vector3(3.4f, 0.85f, 3.4f);
    }
}