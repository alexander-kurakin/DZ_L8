using UnityEngine;

namespace Assets._Project.Develop.Runtime.Utilities.Audio
{
    public interface IGameSoundsService
    {
        void PlayGlobal(GameSoundsIDs id);
        void PlayLocal(AudioSource localSource,  GameSoundsIDs id);
    }
}