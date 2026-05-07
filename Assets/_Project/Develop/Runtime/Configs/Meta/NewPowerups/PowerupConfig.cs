using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Meta.NewPowerups
{
    public abstract class PowerupConfig : ScriptableObject
    {
        [field: SerializeField] public string ID { get; private set; }
        public abstract int MaxLevel { get; }

        //meta-data 
        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField] public string Description { get; private set; }
        [field: SerializeField] public Sprite Icon { get; private set; }
        [field: SerializeField] public int CostInDiamonds { get; private set; }

        public bool IsUpgradable() => MaxLevel > 1;
    }
}