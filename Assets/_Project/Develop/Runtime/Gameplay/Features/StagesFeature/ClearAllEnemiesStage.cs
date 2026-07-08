using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System;
using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;
using Assets._Project.Develop.Runtime.Utilities.Audio;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature
{
    public class ClearAllEnemiesStage : IStage
    {
        private const float STUB_WAVE_DURATION_SECONDS = 1f;

        private readonly ClearAllEnemiesWaveRuntimeData _waveData;
        private readonly IMouseInputService _mouseInputService;
        private readonly IBackgroundMusicService _backgroundMusicService;

        private readonly ReactiveEvent _completed = new();

        private float _elapsedSeconds;
        private bool _inProcess;

        public ClearAllEnemiesStage(
            ClearAllEnemiesWaveRuntimeData waveData,
            IMouseInputService mouseInputService,
            IBackgroundMusicService backgroundMusicService)
        {
            _waveData = waveData;
            _mouseInputService = mouseInputService;
            _backgroundMusicService = backgroundMusicService;
        }

        public IReadOnlyEvent Completed => _completed;

        public void Start()
        {
            if (_inProcess)
                throw new InvalidOperationException("Game mode already started");

            _backgroundMusicService.Play(BackgroundMusicTrackIDs.Battle);
            _mouseInputService.HideCursor();
            _elapsedSeconds = 0f;
            _inProcess = true;
        }

        public void Update(float deltaTime)
        {
            if (_inProcess == false)
                return;

            _elapsedSeconds += deltaTime;

            if (_elapsedSeconds >= STUB_WAVE_DURATION_SECONDS)
                ProcessEnd();
        }

        public void Cleanup()
        {
            _inProcess = false;
            _elapsedSeconds = 0f;
        }

        public void Dispose()
        {
            Cleanup();
        }

        private void ProcessEnd()
        {
            _inProcess = false;
            _mouseInputService.ShowCursor();
            _completed.Invoke();
        }
    }
}
