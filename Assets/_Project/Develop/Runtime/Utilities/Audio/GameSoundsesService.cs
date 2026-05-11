using _Project.Develop.Runtime.Configs.Utilities.Audio;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Utilities.Audio
{
    public class GameSoundsesService : AudioPlaybackService<GameSoundsIDs>, IGameSoundsService
    {
        public GameSoundsesService(
            AudioSource audioSource,
            GameSoundsConfig config) : base(audioSource, config)
        {
        }

        protected override void InitSetup()
        {
            _audioSource.loop = false;
            _audioSource.playOnAwake = false;
        }

        public void Play(GameSoundsIDs id)
        {
            if (TryGetClip(id, out AudioClip clipToPlay) == false)
                return;

            _audioSource.PlayOneShot(clipToPlay);
        }
    }
}