using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using System;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Stages
{
    [Serializable]
    public class EnemyItemConfig
    {
        [field: SerializeField] public EntityConfig EnemyConfig { get; private set; }
        [field: SerializeField] public int EnemiesCount { get; private set; }
    }
}
