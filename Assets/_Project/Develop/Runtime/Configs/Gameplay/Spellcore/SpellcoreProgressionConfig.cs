using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Spellcore
{
    [CreateAssetMenu(menuName = "Configs/Gameplay/Spellcore/NewSpellcoreProgressionConfig", fileName = "SpellcoreProgressionConfig")]
    public class SpellcoreProgressionConfig : ScriptableObject
    {
        [SerializeField] private int[] _pathCountsByWave = { 1, 2, 3, 5, 5 };
        [SerializeField, Min(1)] private int _maxPathCount = 16;
        [SerializeField, Min(1)] private int _survivalPathsPerWave = 1;
        [SerializeField, Min(0)] private int _starterFreeMines = 2;

        public int MaxPathCount => _maxPathCount;

        public int SurvivalPathsPerWave => _survivalPathsPerWave;

        public int CampaignWaveCount => _pathCountsByWave.Length;

        public int GetPathCountForWave(int waveNumber)
        {
            if (waveNumber <= 0)
                return 1;

            int waveIndex = waveNumber - 1;

            if (waveIndex < _pathCountsByWave.Length)
                return _pathCountsByWave[waveIndex];

            int campaignPeakPathCount = _pathCountsByWave[_pathCountsByWave.Length - 1];
            int survivalWavesAfterCampaign = waveNumber - _pathCountsByWave.Length;
            int targetPathCount = campaignPeakPathCount + survivalWavesAfterCampaign * _survivalPathsPerWave;

            return Mathf.Min(targetPathCount, _maxPathCount);
        }

        public int StarterFreeMines => _starterFreeMines;
    }
}
