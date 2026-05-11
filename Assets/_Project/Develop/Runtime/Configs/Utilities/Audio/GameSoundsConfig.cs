using System;
using System.Collections.Generic;
using System.Linq;
using Assets._Project.Develop.Runtime.Utilities.Audio;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Project.Develop.Runtime.Configs.Utilities.Audio
{
        [CreateAssetMenu(
            menuName = "Configs/Utilities/Audio/NewGameSoundsConfig",
            fileName = "GameSoundsConfig")]
        public class GameSoundsConfig : ScriptableObject, IAudioClipsConfig<GameSoundsIDs>
        {
            [SerializeField] private List<GameSoundItem> _sounds;

            public AudioClip GetClip(GameSoundsIDs id)
            {
                GameSoundItem soundItem = _sounds.First(item => item.Id == id);
                AudioClip[] clips = soundItem.Clip;
                
                int randomIndex = Random.Range(0, clips.Length);
                return clips[randomIndex];
            }

            [Serializable]
            private class GameSoundItem
            {
                [field: SerializeField] public GameSoundsIDs Id { get; private set; }
                [field: SerializeField] public AudioClip[] Clip { get; private set; }
            }
        }
}