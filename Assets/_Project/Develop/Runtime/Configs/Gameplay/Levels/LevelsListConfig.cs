using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Levels
{
    [CreateAssetMenu(menuName = "Configs/Gameplay/Levels/NewLevelsListConfig", fileName = "LevelsListConfig")]
    public class LevelsListConfig : ScriptableObject
    {
        [SerializeField] private List<LevelConfig> _levels;

        public IReadOnlyList<LevelConfig> Levels => _levels;

        public LevelConfig GetBy(int levelNumber)
        {
            VerifyLevels();
            
            int levelIndex = levelNumber - 1;

            return _levels[levelIndex];
        }

        public int GetRandomLevelNumber()
        {
            VerifyLevels();
            
            return Random.Range(0, _levels.Count) + 1;
        }

        private void VerifyLevels()
        {
            if (_levels.Count == 0 || _levels == null)
                throw new ArgumentOutOfRangeException(nameof(_levels), "Invalid setup of levels");
        }
    }
}
