using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Entities
{
    public class PurchasableEntityConfig : EntityConfig
    {
        [field: SerializeField, Min(0)] public int CostInGold { get; private set; }
    }
}