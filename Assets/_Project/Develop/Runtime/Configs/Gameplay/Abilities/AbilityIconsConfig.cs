using System;
using System.Collections.Generic;
using System.Linq;
using Assets._Project.Develop.Runtime.Gameplay.Features.Ability;
using UnityEngine;

namespace _Project.Develop.Runtime.Configs.Gameplay.Abilities
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