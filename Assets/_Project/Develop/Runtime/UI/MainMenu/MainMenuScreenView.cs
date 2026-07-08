using Assets._Project.Develop.Runtime.UI.Core;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Project.Develop.Runtime.UI.MainMenu
{
    public class MainMenuScreenView : MonoBehaviour, IView
    {
        public event Action PlayButtonClicked;

        [SerializeField] private Button _playButton;

        private void OnEnable()
        {
            if (_playButton != null)
                _playButton.onClick.AddListener(OnPlayButtonClicked);
        }

        private void OnDisable()
        {
            if (_playButton != null)
                _playButton.onClick.RemoveListener(OnPlayButtonClicked);
        }

        private void OnPlayButtonClicked() => PlayButtonClicked?.Invoke();
    }
}
