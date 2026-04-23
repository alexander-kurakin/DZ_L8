using System;
using System.Collections.Generic;
using System.Linq;
using Assets._Project.Develop.Runtime.Configs.Meta.Stats;
using Assets._Project.Develop.Runtime.Meta.Features.Wallet;
using UnityEngine;

namespace _Project.Develop.Runtime.Configs.Meta.Stats
{
    [CreateAssetMenu(menuName = "Configs/Meta/Stats/NewStatIconsConfig", fileName = "StatIconsConfig")]
    public class StatIconsConfig : ScriptableObject
    {
        [SerializeField] private List<StatConfig> _configs;

        public Sprite GetSpriteFor(StatType statType)
            => _configs.First(config => config.Type == statType).Sprite;

        [Serializable]
        private class StatConfig
        {
            [field: SerializeField] public StatType Type { get; private set; }
            [field: SerializeField] public Sprite Sprite { get; private set; }
        }
    }
}