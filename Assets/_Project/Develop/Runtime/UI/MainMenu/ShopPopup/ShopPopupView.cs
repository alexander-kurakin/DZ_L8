using System;
using Assets._Project.Develop.Runtime.UI.CommonViews;
using Assets._Project.Develop.Runtime.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Assets._Project.Develop.Runtime.UI.MainMenu.ShopPopup
{
    public class ShopPopupView : PopupViewBase
    {
        [SerializeField] private TMP_Text _title;
        [SerializeField] private IconTextView _towerHealInnerTextView;
        [SerializeField] private IconTextView _enemiesDebuffInnerTextView;
        [SerializeField] private IconTextView _powerfulClickInnerTextView;
        
        [SerializeField] private IconView _towerHealOuterView;
        [SerializeField] private IconView _enemiesDebuffOuterView;
        [SerializeField] private IconView _powerfulClickOuterView;

        public void SetTitle(string title) => _title.text = title;
        
        public IconView TowerHealOuterView => _towerHealOuterView;
        public IconView EnemiesDebuffOuterView => _enemiesDebuffOuterView;
        public IconView PowerfulClickOuterView => _powerfulClickOuterView;

        public IconTextView TowerHealInnerTextView =>  _towerHealInnerTextView;
        public IconTextView EnemiesDebuffInnerTextView => _enemiesDebuffInnerTextView;
        public IconTextView PowerfulClickInnerTextView => _powerfulClickInnerTextView;
    }
}