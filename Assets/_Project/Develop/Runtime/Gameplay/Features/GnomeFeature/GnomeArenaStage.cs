using Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature;
using Assets._Project.Develop.Runtime.Utilities.Audio;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.GnomeFeature
{
    public class GnomeArenaStage : IStage
    {
        private readonly IBackgroundMusicService _backgroundMusicService;
        private readonly GnomeWinConditionService _gnomeWinConditionService;

        private readonly ReactiveEvent _completed = new();

        private IDisposable _winSubscription;
        private bool _inProcess;

        public GnomeArenaStage(
            IBackgroundMusicService backgroundMusicService,
            GnomeWinConditionService gnomeWinConditionService)
        {
            _backgroundMusicService = backgroundMusicService;
            _gnomeWinConditionService = gnomeWinConditionService;
        }

        public IReadOnlyEvent Completed => _completed;

        public void Start()
        {
            if (_inProcess == true)
                throw new InvalidOperationException("Gnome arena stage already started.");

            _backgroundMusicService.Play(BackgroundMusicTrackIDs.Battle);
            _winSubscription = _gnomeWinConditionService.Completed.Subscribe(OnWinConditionCompleted);
            _inProcess = true;
        }

        public void Update(float deltaTime)
        {
        }

        public void Cleanup()
        {
            _inProcess = false;
            _winSubscription?.Dispose();
            _winSubscription = null;
        }

        public void Dispose()
        {
            Cleanup();
        }

        private void OnWinConditionCompleted()
        {
            if (_inProcess == false)
                return;

            _inProcess = false;
            _completed.Invoke();
        }
    }
}
