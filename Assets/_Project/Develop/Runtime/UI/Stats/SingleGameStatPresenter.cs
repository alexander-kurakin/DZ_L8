using System;
using _Project.Develop.Runtime.Configs.Meta.Stats;
using Assets._Project.Develop.Runtime.Configs.Meta.Stats;
using Assets._Project.Develop.Runtime.UI.CommonViews;
using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.Utilities.Reactive;

namespace Assets._Project.Develop.Runtime.UI.Stats
{
    public class SingleGameStatPresenter : IPresenter
    {
        private readonly IReadOnlyVariable<int> _gameStat;
        private readonly StatType _statType;
        private readonly StatIconsConfig _statIconsConfig; 
        
        private readonly IconTextView _view;
        private IDisposable _disposable;

        public SingleGameStatPresenter(
            IReadOnlyVariable<int> gameStat,
            StatType statType,
            StatIconsConfig statIconsConfig,
            IconTextView view)
        {
            _gameStat = gameStat;
            _statType = statType;
            _statIconsConfig = statIconsConfig;
            _view = view;
        }
        
        public IconTextView View => _view;

        public void Initialize()
        {
            UpdateValue(_gameStat.Value);
            _view.SetIcon(_statIconsConfig.GetSpriteFor(_statType));

            _disposable = _gameStat.Subscribe(OnStatChanged);
        }
        
        public void Dispose()
        {
            _disposable?.Dispose();
        }

        private void OnStatChanged(int arg1, int newValue) => UpdateValue(newValue);

        private void UpdateValue(int value) => _view.SetText(value.ToString());
    }
}