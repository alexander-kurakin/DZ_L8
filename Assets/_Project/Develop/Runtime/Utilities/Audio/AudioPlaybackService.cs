using UnityEngine;

namespace Assets._Project.Develop.Runtime.Utilities.Audio
{
    public abstract class AudioPlaybackService<TId>
    {
        protected readonly AudioSource _audioSource;
        private readonly IAudioClipsConfig<TId> _config;

        protected AudioPlaybackService(
            AudioSource audioSource,
            IAudioClipsConfig<TId> config)
        {
            _audioSource = audioSource;
            _config = config;
            
            InitSetup();
        }

        protected abstract void InitSetup();
        
        public void SetVolume(float volume)
        {
            _audioSource.volume = Mathf.Clamp01(volume);
        }
        
        protected bool TryGetClip(TId id, out AudioClip clip)
        {
            clip = _config.GetClip(id);
            
            if (clip != null)
                return true;
            
            Debug.Log($"Audio clip is null for id: {id}");
            
            return false;
        }
    }
}