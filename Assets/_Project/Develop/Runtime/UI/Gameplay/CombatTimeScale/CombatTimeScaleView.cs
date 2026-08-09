using System;
using Assets._Project.Develop.Runtime.Gameplay.Features.GameplayTimeScale;
using Assets._Project.Develop.Runtime.UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Develop.Runtime.UI.Gameplay.CombatTimeScale
{
    public class CombatTimeScaleView : MonoBehaviour, IView
    {
        private static readonly Color SELECTED_COLOR = new Color(0.49411765f, 0.7843137f, 0.9098039f, 0.9f);
        private static readonly Color DEFAULT_COLOR = new Color(0.101960786f, 0.14117648f, 0.2f, 0.9f);

        [SerializeField] private Button _pauseButton;
        [SerializeField] private Button _normalSpeedButton;
        [SerializeField] private Button _fastSpeedButton;

        public event Action PauseClicked;
        public event Action NormalSpeedClicked;
        public event Action FastSpeedClicked;

        private void OnEnable()
        {
            _pauseButton.onClick.AddListener(OnPauseClicked);
            _normalSpeedButton.onClick.AddListener(OnNormalSpeedClicked);
            _fastSpeedButton.onClick.AddListener(OnFastSpeedClicked);
        }

        private void OnDisable()
        {
            _pauseButton.onClick.RemoveListener(OnPauseClicked);
            _normalSpeedButton.onClick.RemoveListener(OnNormalSpeedClicked);
            _fastSpeedButton.onClick.RemoveListener(OnFastSpeedClicked);
        }

        public void SetSelectedMode(GameplayTimeScaleMode mode)
        {
            SetButtonSelected(_pauseButton, mode == GameplayTimeScaleMode.Paused);
            SetButtonSelected(_normalSpeedButton, mode == GameplayTimeScaleMode.Normal);
            SetButtonSelected(_fastSpeedButton, mode == GameplayTimeScaleMode.Fast);
        }

        private void OnPauseClicked() => PauseClicked?.Invoke();

        private void OnNormalSpeedClicked() => NormalSpeedClicked?.Invoke();

        private void OnFastSpeedClicked() => FastSpeedClicked?.Invoke();

        private static void SetButtonSelected(Button button, bool isSelected)
        {
            if (button == null)
                return;

            Image image = button.targetGraphic as Image;

            if (image == null)
                return;

            image.color = isSelected ? SELECTED_COLOR : DEFAULT_COLOR;
        }
    }
}
