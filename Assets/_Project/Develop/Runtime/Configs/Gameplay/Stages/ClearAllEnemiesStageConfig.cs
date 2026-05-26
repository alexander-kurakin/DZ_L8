using System.Collections.Generic;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Stages
{
    [CreateAssetMenu(menuName = "Configs/Gameplay/Stages/NewClearAllEnemiesStage", fileName = "ClearAllEnemiesStage")]
    public class ClearAllEnemiesStageConfig : StageConfig
    {
        [SerializeField] private List<EnemyItemConfig> _enemyItems;

        public IReadOnlyList<EnemyItemConfig> EnemyItems => _enemyItems;
        [field: SerializeField] public float EnemySpawnRadius { get; private set; }

        public int EnemiesCount
        {
            get
            {
                int count = 0;
                
                foreach (EnemyItemConfig enemyItem in _enemyItems)
                    count += enemyItem.EnemiesCount;
                
                return count;
            }            
        }
    }
}
