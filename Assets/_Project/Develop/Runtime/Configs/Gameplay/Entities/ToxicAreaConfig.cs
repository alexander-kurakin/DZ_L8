using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Entities
{
    [CreateAssetMenu(menuName = "Configs/Gameplay/Entities/NewToxicAreaConfig", fileName = "ToxicAreaConfig")]
    public class ToxicAreaConfig : PurchasableEntityConfig
    {
        [field: SerializeField, Min(0)] public float DamagePerTick { get; private set; } = 10;
        [field: SerializeField, Min(0)] public float Radius { get; private set; } = 4;
        [field: SerializeField, Min(0)] public float TickCooldown { get; private set; } = 0.33f;
    }
}