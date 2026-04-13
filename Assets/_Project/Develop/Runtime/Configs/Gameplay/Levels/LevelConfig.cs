using Assets._Project.Develop.Runtime.Configs.Gameplay.Stages;
using System.Collections.Generic;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Levels
{
    [CreateAssetMenu(menuName = "Configs/Gameplay/Levels/NewLevelConfig", fileName = "LevelConfig")]
    public class LevelConfig : ScriptableObject
    {
        [SerializeField] private List<StageConfig> _stageConfigs;
        [SerializeField] private float _towerMaxHealth;
        [SerializeField] private int _goldReward;

        public IReadOnlyList<StageConfig> StageConfigs => _stageConfigs;
        public float TowerMaxHealth => _towerMaxHealth;
        
        public int GoldReward => _goldReward;
    }
}
