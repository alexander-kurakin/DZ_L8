using Assets._Project.Develop.Runtime.UI.CommonViews;
using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.UI.Gameplay.PlantBuildingBuff
{
    public class PlantBuildingBuffCountdownPresenter : IPresenter
    {
        private readonly ReactiveVariable<float> _remainingSeconds;
        private readonly TextView _textView;

        private IDisposable _remainingSecondsDisposable;

        public PlantBuildingBuffCountdownPresenter(ReactiveVariable<float> remainingSeconds, TextView textView)
        {
            _remainingSeconds = remainingSeconds;
            _textView = textView;
        }

        public TextView View => _textView;

        public void Initialize()
        {
            _remainingSecondsDisposable = _remainingSeconds.Subscribe(OnRemainingSecondsChanged);
            OnRemainingSecondsChanged(0f, _remainingSeconds.Value);
        }

        public void Dispose()
        {
            _remainingSecondsDisposable?.Dispose();
        }

        private void OnRemainingSecondsChanged(float oldValue, float newValue)
        {
            int secondsLeft = Mathf.RoundToInt(newValue);

            if (secondsLeft < 0)
                secondsLeft = 0;

            _textView.SetText(secondsLeft.ToString());
        }
    }
}
