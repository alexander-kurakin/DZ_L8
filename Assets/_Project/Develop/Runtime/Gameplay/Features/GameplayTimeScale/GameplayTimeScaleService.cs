using System;
using DG.Tweening;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.GameplayTimeScale
{
    public enum GameplayTimeScaleMode
    {
        Paused = 0,
        Normal = 1,
        Fast = 3,
    }

    public class GameplayTimeScaleService
    {
        private GameplayTimeScaleMode _currentMode = GameplayTimeScaleMode.Normal;

        public event Action Changed;

        public GameplayTimeScaleMode CurrentMode => _currentMode;

        public void SetMode(GameplayTimeScaleMode mode)
        {
            if (_currentMode == mode)
                return;

            bool wasPaused = _currentMode == GameplayTimeScaleMode.Paused;
            _currentMode = mode;
            UnityEngine.Time.timeScale = (float)mode;
            ApplyDotweenPauseState(wasPaused, mode == GameplayTimeScaleMode.Paused);
            Changed?.Invoke();
        }

        public void ResetToNormal()
        {
            bool wasPaused = _currentMode == GameplayTimeScaleMode.Paused;
            _currentMode = GameplayTimeScaleMode.Normal;
            UnityEngine.Time.timeScale = 1f;
            ApplyDotweenPauseState(wasPaused, isPaused: false);
            Changed?.Invoke();
        }

        private static void ApplyDotweenPauseState(bool wasPaused, bool isPaused)
        {
            if (isPaused)
            {
                DOTween.PauseAll();
                return;
            }

            if (wasPaused)
                DOTween.PlayAll();
        }
    }
}
