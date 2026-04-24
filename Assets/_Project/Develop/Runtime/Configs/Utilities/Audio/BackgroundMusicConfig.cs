using System;
using System.Collections.Generic;
using System.Linq;
using Assets._Project.Develop.Runtime.Utilities.Audio;
using UnityEngine;

namespace _Project.Develop.Runtime.Configs.Utilities.Audio
{
    [CreateAssetMenu(
        menuName = "Configs/Utilities/Audio/NewBackgroundMusicConfig",
        fileName = "BackgroundMusicConfig")] 
    public class BackgroundMusicConfig : ScriptableObject, IAudioClipsConfig<BackgroundMusicTrackIDs>
    {
        [SerializeField] private List<BackgroundMusicItem> _tracks;
        
        public AudioClip GetClip(BackgroundMusicTrackIDs id)
            => _tracks.First(item => item.Id == id).Clip;
        
        [Serializable]
        private class BackgroundMusicItem
        {
            [field: SerializeField] public BackgroundMusicTrackIDs Id { get; private set; }
            [field: SerializeField] public AudioClip Clip { get; private set; }
        }
    }
}