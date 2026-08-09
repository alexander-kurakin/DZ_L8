using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System.Collections.Generic;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Ability
{
    public class LmbFlavorToastService
    {
        private readonly ReactiveEvent<LmbFlavorToastType> _toastRequested = new();
        private readonly HashSet<LmbFlavorToastType> _shownOnceTypes = new();
        private readonly Dictionary<LmbFlavorToastType, float> _nextAllowedShowUnscaledTimeByType = new();

        public ReactiveEvent<LmbFlavorToastType> ToastRequested => _toastRequested;

        public void Show(LmbFlavorToastType toastType)
        {
            _toastRequested.Invoke(toastType);
        }

        public void ShowOnce(LmbFlavorToastType toastType)
        {
            if (_shownOnceTypes.Add(toastType) == false)
                return;

            Show(toastType);
        }

        public void ShowThrottled(LmbFlavorToastType toastType, float cooldownSeconds)
        {
            float now = Time.unscaledTime;

            if (_nextAllowedShowUnscaledTimeByType.TryGetValue(toastType, out float nextAllowedTime)
                && now < nextAllowedTime)
                return;

            _nextAllowedShowUnscaledTimeByType[toastType] = now + cooldownSeconds;
            Show(toastType);
        }

        public void ResetForRun()
        {
            _shownOnceTypes.Clear();
            _nextAllowedShowUnscaledTimeByType.Clear();
        }
    }
}
