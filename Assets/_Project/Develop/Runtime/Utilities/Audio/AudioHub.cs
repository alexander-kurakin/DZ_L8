using UnityEngine;

namespace Assets._Project.Develop.Runtime.Utilities.Audio
{
    public class AudioHub : MonoBehaviour
    {
        [SerializeField] private AudioSource _backgroundMusicSource;
        [SerializeField] private AudioSource _uiSoundsSource;

        public AudioSource BackgroundMusicSource => _backgroundMusicSource;
        public AudioSource UISoundsSource => _uiSoundsSource;

        private void Awake()
        {
            DontDestroyOnLoad(this);
        }
        
    }
}