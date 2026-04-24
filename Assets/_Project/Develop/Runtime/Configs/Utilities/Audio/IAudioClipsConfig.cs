using UnityEngine;

namespace Assets._Project.Develop.Runtime.Utilities.Audio
{
    public interface IAudioClipsConfig<TId>
    {
        AudioClip GetClip(TId id);
    }
}