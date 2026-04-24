using System;
using System.Collections.Generic;
using System.Linq;
using Assets._Project.Develop.Runtime.Utilities.Audio;
using UnityEngine;

namespace _Project.Develop.Runtime.Configs.Utilities.Audio
{
    [CreateAssetMenu(
        menuName = "Configs/Utilities/Audio/NewUISoundsConfig",
        fileName = "UISoundsConfig")]
    public class UISoundsConfig : ScriptableObject, IAudioClipsConfig<UISoundIDs>
    {
        [SerializeField] private List<UISoundItem> _sounds;
        
        public AudioClip GetClip(UISoundIDs id)
            => _sounds.First(item => item.Id == id).Clip;
        
        [Serializable]
        private class UISoundItem
        {
            [field: SerializeField] public UISoundIDs Id { get; private set; }
            [field: SerializeField] public AudioClip Clip { get; private set; }
        }
    }
}