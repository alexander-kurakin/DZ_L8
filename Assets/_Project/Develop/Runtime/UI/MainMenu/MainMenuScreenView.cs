using Assets._Project.Develop.Runtime.UI.CommonViews;
using Assets._Project.Develop.Runtime.UI.Core;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Project.Develop.Runtime.UI.MainMenu
{
    public class MainMenuScreenView : MonoBehaviour, IView
    {
        public event Action PlayButtonClicked;
        public event Action ShopButtonClicked;

        [field: SerializeField] public IconTextListView WalletIconTextListView { get; private set; }

        [field: SerializeField] public IconTextListView StatsIconTextListView { get; private set; }

        [SerializeField] private Button _playButton;
        [SerializeField] private Button _shopButton;

        private void OnEnable()
        {
            _playButton.onClick.AddListener(OnPlayButtonClicked);
            _shopButton.onClick.AddListener(OnShopButtonClicked);
        }

        private void OnShopButtonClicked() => ShopButtonClicked?.Invoke();

        private void OnDisable()
        {
            _playButton.onClick.RemoveListener(OnPlayButtonClicked);
            _shopButton.onClick.RemoveListener(OnShopButtonClicked);
        }

        private void OnPlayButtonClicked() => PlayButtonClicked?.Invoke();
    }
}
