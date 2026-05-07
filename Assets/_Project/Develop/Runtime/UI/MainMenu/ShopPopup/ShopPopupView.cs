using System;
using Assets._Project.Develop.Runtime.UI.Core;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Project.Develop.Runtime.UI.MainMenu.ShopPopup
{
    public class ShopPopupView : PopupViewBase
    {
        public event Action BuyButtonClicked;        
        
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _selectAbilityText;
        [SerializeField] private Button _buyButton;

        [SerializeField] private SelectableAbilityListView _abilityListView;
        
        public SelectableAbilityListView AbilityListView => _abilityListView;

        private void OnEnable()
        {
            _buyButton.onClick.AddListener(OnBuyButtonClicked);
        }

        private void OnBuyButtonClicked() => BuyButtonClicked?.Invoke(); 

        private void OnDisable()
        {
            _buyButton.onClick.RemoveListener(OnBuyButtonClicked);
        }
        
        public void SetTitle(string title) => _title.text = title;
        
        public void BuyButtonOn() => _buyButton.gameObject.SetActive(true);

        public void BuyButtonOff() => _buyButton.gameObject.SetActive(false);

        public void SetAdditionalText(string additionalText) => _selectAbilityText.text = additionalText;
        
        protected override void ModifyShowAnimation(Sequence animation)
        {
            base.ModifyShowAnimation(animation);

            animation.Append(_abilityListView.Show());
        }

        protected override void ModifyHideAnimation(Sequence animation)
        {
            base.ModifyHideAnimation(animation);

            animation.Append(_abilityListView.Hide());
        }
    }
}