using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Spellcore
{
    [CreateAssetMenu(menuName = "Configs/Gameplay/Spellcore/NewSpellcoreProgressionConfig", fileName = "SpellcoreProgressionConfig")]
    public class SpellcoreProgressionConfig : ScriptableObject
    {
        [SerializeField] private int[] _pathCountsByWave = { 1, 2, 4, 6, 6 };
        [SerializeField, Min(0)] private int _starterFreeMines = 2;

        public int GetPathCountForWave(int waveNumber)
        {
            if (waveNumber <= 0)
                return 1;

            int waveIndex = waveNumber - 1;

            if (waveIndex >= _pathCountsByWave.Length)
                return _pathCountsByWave[_pathCountsByWave.Length - 1];

            return _pathCountsByWave[waveIndex];
        }

        public int StarterFreeMines => _starterFreeMines;
    }
}
