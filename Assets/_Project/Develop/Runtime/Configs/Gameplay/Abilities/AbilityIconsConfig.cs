using System;
using System.Collections.Generic;
using System.Linq;
using Assets._Project.Develop.Runtime.Gameplay.Features.Ability;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Abilities
{
    [CreateAssetMenu(menuName = "Configs/Gameplay/Abilities/NewAbilityIconsConfig", fileName = "AbilityIconsConfig")]
    public class AbilityIconsConfig : ScriptableObject
    {
        [SerializeField] private List<AbilityConfig> _configs;

        public Sprite GetSpriteFor(AbilityType abilityType)
            => _configs.First(config => config.Type == abilityType).Sprite;

        [Serializable]
        private class AbilityConfig
        {
            [field: SerializeField] public AbilityType Type { get; private set; }
            [field: SerializeField] public Sprite Sprite { get; private set; }
        }
    }
}

public abstract class AbilityConfig : ScriptableObject
{
    [field: SerializeField] public string ID { get; private set; }
    public abstract int MaxLevel { get; }

    //meta-data 
    [field: SerializeField] public string Name { get; private set; }
    [field: SerializeField] public string Description { get; private set; }
    [field: SerializeField] public Sprite Icon { get; private set; }

    public bool IsUpgradable() => MaxLevel > 1;
}