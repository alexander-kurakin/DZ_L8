using _Project.Develop.Runtime.Configs.Utilities.Audio;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Utilities.Audio
{
    public class BackgroundMusicService : AudioPlaybackService<BackgroundMusicTrackIDs>, IBackgroundMusicService
    {
        private BackgroundMusicTrackIDs _currentId;
        
        public BackgroundMusicService(
            AudioSource audioSource,
            BackgroundMusicConfig config) : base(audioSource, config)
        {
        }

        protected override void InitSetup()
        {
            _audioSource.loop = true;
            _audioSource.playOnAwake = false;
        }

        public void Play(BackgroundMusicTrackIDs id)
        {
            if (_currentId == id && _audioSource.isPlaying)
                return;
            
            if (TryGetClip(id, out AudioClip clipToPlay) == false)
                return;
            
            _currentId = id;
            
            _audioSource.clip = clipToPlay;
            _audioSource.Play();
        }
    }
}