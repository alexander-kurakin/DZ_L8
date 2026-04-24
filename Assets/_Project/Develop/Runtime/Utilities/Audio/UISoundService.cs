using _Project.Develop.Runtime.Configs.Utilities.Audio;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Utilities.Audio
{
    public class UISoundService : AudioPlaybackService<UISoundIDs>, IUISoundService
    {
      
        public UISoundService(
            AudioSource audioSource,
            UISoundsConfig config) : base(audioSource,config)
        {
        }

        protected override void InitSetup()
        {
            _audioSource.loop = false;
            _audioSource.playOnAwake = false;
        }

        public void Play(UISoundIDs id)
        {
            if (TryGetClip(id, out AudioClip clipToPlay) == false)
                return;
            
            _audioSource.PlayOneShot(clipToPlay);
        }
    }
}