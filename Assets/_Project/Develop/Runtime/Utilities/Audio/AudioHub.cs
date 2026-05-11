using UnityEngine;

namespace Assets._Project.Develop.Runtime.Utilities.Audio
{
    public class AudioHub : MonoBehaviour
    {
        [SerializeField] private AudioSource _backgroundMusicSource;
        [SerializeField] private AudioSource _uiSoundsSource;
        [SerializeField] private AudioSource _gameSoundSource;

        public AudioSource BackgroundMusicSource => _backgroundMusicSource;
        public AudioSource UISoundsSource => _uiSoundsSource;
        
        public AudioSource GameSoundSource => _gameSoundSource;

        private void Awake()
        {
            DontDestroyOnLoad(this);
        }
        
    }
}