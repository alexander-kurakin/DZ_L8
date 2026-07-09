using System;
using Assets._Project.Develop.Runtime.Gameplay.Features.RunKillCounter;
using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.Utilities.Reactive;

namespace _Project.Develop.Runtime.UI.Gameplay.GnomeKillToast
{
    public class GnomeKillToastPresenter : IPresenter
    {
        private readonly GnomeKillToastView _view;
        private readonly RunEnemyKillCounterService _runEnemyKillCounterService;

        private IDisposable _killCountChangedDisposable;

        public GnomeKillToastPresenter(
            GnomeKillToastView view,
            RunEnemyKillCounterService runEnemyKillCounterService)
        {
            _view = view;
            _runEnemyKillCounterService = runEnemyKillCounterService;
        }

        public void Initialize()
        {
            _killCountChangedDisposable = _runEnemyKillCounterService.KillCount.Subscribe(OnKillCountChanged);
        }

        public void Dispose()
        {
            _killCountChangedDisposable?.Dispose();
        }

        private void OnKillCountChanged(int oldValue, int newValue)
        {
            if (newValue <= oldValue)
                return;

            _view.Play();
        }
    }
}
