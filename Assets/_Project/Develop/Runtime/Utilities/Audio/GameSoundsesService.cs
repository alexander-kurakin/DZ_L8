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

        public void Play(GameSoundsIDs soundID, AudioSource localAudioSource = null)
        {
            if (TryGetClip(soundID, out AudioClip clipToPlay) == false)
                return;
            
            AudioSource finalAudioSource = 
                localAudioSource==null ? _audioSource : localAudioSource;
            
            finalAudioSource.clip = clipToPlay;
            finalAudioSource.Play();
        }
        
        public void PlayOneShot(GameSoundsIDs soundID, AudioSource localAudioSource = null)
        {
            if (TryGetClip(soundID, out AudioClip clipToPlay) == false)
                return;

            AudioSource finalAudioSource = 
                localAudioSource==null ? _audioSource : localAudioSource;
            
            finalAudioSource.PlayOneShot(clipToPlay);
        }
    }
}