using System;
using Assets._Project.Develop.Runtime.Configs.Meta.Stats;
using Assets._Project.Develop.Runtime.Gameplay.Features.RunKillCounter;
using Assets._Project.Develop.Runtime.UI.CommonViews;
using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.Utilities.Reactive;

namespace Assets._Project.Develop.Runtime.UI.Stats
{
    public class GameplayStatsPresenter : IPresenter
    {
        private readonly IconTextListView _statsView;
        private readonly ViewsFactory _viewsFactory;
        private readonly RunEnemyKillCounterService _runEnemyKillCounterService;
        private readonly StatIconsConfig _statIconsConfig;

        private IconTextView _killCounterItem;
        private IDisposable _killCountChangedDisposable;

        public GameplayStatsPresenter(
            IconTextListView statsView,
            ViewsFactory viewsFactory,
            RunEnemyKillCounterService runEnemyKillCounterService,
            StatIconsConfig statIconsConfig)
        {
            _statsView = statsView;
            _viewsFactory = viewsFactory;
            _runEnemyKillCounterService = runEnemyKillCounterService;
            _statIconsConfig = statIconsConfig;
        }

        public void Initialize()
        {
            _killCounterItem = _viewsFactory.Create<IconTextView>(ViewIDs.StatIconTextView);
            _statsView.Add(_killCounterItem);

            _killCounterItem.SetIcon(_statIconsConfig.GetSpriteFor(StatType.Losses));
            UpdateKillCount(_runEnemyKillCounterService.KillCount.Value);

            _killCountChangedDisposable = _runEnemyKillCounterService.KillCount.Subscribe(OnKillCountChanged);
        }

        public void Dispose()
        {
            _killCountChangedDisposable?.Dispose();

            if (_killCounterItem == null)
                return;

            _statsView.Remove(_killCounterItem);
            _viewsFactory.Release(_killCounterItem);
            _killCounterItem = null;
        }

        private void OnKillCountChanged(int oldValue, int newValue) => UpdateKillCount(newValue);

        private void UpdateKillCount(int killCount) => _killCounterItem.SetText(killCount.ToString());
    }
}
