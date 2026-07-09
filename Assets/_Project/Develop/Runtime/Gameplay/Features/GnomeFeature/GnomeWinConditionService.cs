using Assets._Project.Develop.Runtime.Configs.Gameplay.Gnome;
using Assets._Project.Develop.Runtime.Gameplay.Features.RunKillCounter;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.GnomeFeature
{
    public class GnomeWinConditionService : IDisposable
    {
        private readonly RunEnemyKillCounterService _runEnemyKillCounterService;
        private readonly GnomeArenaConfig _arenaConfig;
        private readonly ReactiveEvent _completed = new();

        private IDisposable _killCountSubscription;
        private bool _isCompleted;

        public GnomeWinConditionService(
            RunEnemyKillCounterService runEnemyKillCounterService,
            GnomeArenaConfig arenaConfig)
        {
            _runEnemyKillCounterService = runEnemyKillCounterService;
            _arenaConfig = arenaConfig;
        }

        public IReadOnlyEvent Completed => _completed;

        public void InitializeForRun()
        {
            _isCompleted = false;
            _killCountSubscription?.Dispose();
            _killCountSubscription = _runEnemyKillCounterService.KillCount.Subscribe(OnKillCountChanged);
        }

        public void Dispose()
        {
            _killCountSubscription?.Dispose();
        }

        private void OnKillCountChanged(int oldValue, int killCount)
        {
            if (_isCompleted == true)
                return;

            if (killCount < _arenaConfig.TargetGnomesToKill)
                return;

            _isCompleted = true;
            _completed.Invoke();
        }
    }
}
