namespace Assets._Project.Develop.Runtime.Utilities.Audio
{
    public interface IBackgroundMusicService
    {
        void Play(BackgroundMusicTrackIDs id);
        void Stop();
    }
}