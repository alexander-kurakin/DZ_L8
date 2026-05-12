using UnityEngine;

namespace Assets._Project.Develop.Runtime.Utilities.Audio
{
    public interface IGameSoundsService
    {
        void Play(GameSoundsIDs soundID, AudioSource localAudioSource = null);
        void PlayOneShot(GameSoundsIDs soundID, AudioSource localAudioSource = null);
    }
}