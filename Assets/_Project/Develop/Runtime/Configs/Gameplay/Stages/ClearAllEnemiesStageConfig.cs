using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Stages
{
    [CreateAssetMenu(menuName = "Configs/Gameplay/Stages/NewClearAllEnemiesStage", fileName = "ClearAllEnemiesStage")]
    public class ClearAllEnemiesStageConfig : StageConfig
    {
        [SerializeField] private List<SpawnGroupConfig> _spawnGroups;

        public IReadOnlyList<SpawnGroupConfig> SpawnGroups => _spawnGroups;
        [field: SerializeField] public float EnemySpawnRadius { get; private set; }

        public int EnemiesCount
        {
            get
            {
                int count = 0;
                
                foreach (SpawnGroupConfig group in _spawnGroups)
                    foreach (EnemyItemConfig item in group.EnemyItems)
                        count += item.EnemiesCount;
                
                return count;
            }            
        }
    }

    [Serializable]
    public class SpawnGroupConfig
    {
        public const int UNSET_PATH_INDEX = -1;

        public List<EnemyItemConfig> EnemyItems;
        public float MinTimeBetweenSpawns;
        public float MaxTimeBetweenSpawns;
        public float PauseAfterGroup;
        public int PathIndex = UNSET_PATH_INDEX;
    }
}
