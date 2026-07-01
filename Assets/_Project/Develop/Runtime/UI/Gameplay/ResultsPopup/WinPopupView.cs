using Assets._Project.Develop.Runtime.UI.Core;
using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Project.Develop.Runtime.UI.Gameplay.ResultsPopups
{
    public class WinPopupView : PopupViewBase
    {
        public event Action ContinueClicked;
        public event Action SecondaryClicked;

        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _goldReward;
        [SerializeField] private List<Transform> _stars;
        [SerializeField] private GameObject _standardContinueRoot;
        [SerializeField] private Button _primaryActionButton;
        [SerializeField] private TMP_Text _primaryActionLabel;
        [SerializeField] private Button _secondaryActionButton;
        [SerializeField] private TMP_Text _secondaryActionLabel;

        public void SetTitle(string title) => _title.text = title;

        public void SetGoldReward(int goldReward)
        {
            if (goldReward > 0)
            {
                _goldReward.gameObject.SetActive(true);
                _goldReward.text = goldReward.ToString();
            }
            else
            {
                _goldReward.gameObject.SetActive(false);
            }
        }

        public void SetMode(WinPopupMode mode)
        {
            bool useSurvivalActions = mode != WinPopupMode.Standard;

            if (_standardContinueRoot != null)
                _standardContinueRoot.SetActive(useSurvivalActions == false);

            if (_primaryActionButton != null)
                _primaryActionButton.gameObject.SetActive(useSurvivalActions);

            if (_secondaryActionButton != null)
                _secondaryActionButton.gameObject.SetActive(useSurvivalActions);

            if (useSurvivalActions == false)
                return;

            switch (mode)
            {
                case WinPopupMode.SurvivalOffer:
                    SetTitle("YOU WIN!");
                    SetActionLabels("ENTER SURVIVAL MODE", "MAIN MENU");
                    break;

                case WinPopupMode.SurvivalMilestone:
                    SetTitle("SURVIVAL MILESTONE!");
                    SetActionLabels("CONTINUE", "EXIT");
                    break;
            }
        }

        public void OnContinueClick() => ContinueClicked?.Invoke();

        protected override void OnPreShow()
        {
            base.OnPreShow();

            if (_primaryActionButton != null)
                _primaryActionButton.onClick.AddListener(OnPrimaryActionClicked);

            if (_secondaryActionButton != null)
                _secondaryActionButton.onClick.AddListener(OnSecondaryActionClicked);
        }

        protected override void OnPreHide()
        {
            base.OnPreHide();

            if (_primaryActionButton != null)
                _primaryActionButton.onClick.RemoveListener(OnPrimaryActionClicked);

            if (_secondaryActionButton != null)
                _secondaryActionButton.onClick.RemoveListener(OnSecondaryActionClicked);
        }

        private void OnDisable()
        {
            if (_primaryActionButton != null)
                _primaryActionButton.onClick.RemoveListener(OnPrimaryActionClicked);

            if (_secondaryActionButton != null)
                _secondaryActionButton.onClick.RemoveListener(OnSecondaryActionClicked);
        }

        private void SetActionLabels(string primaryLabel, string secondaryLabel)
        {
            if (_primaryActionLabel != null)
                _primaryActionLabel.text = primaryLabel;

            if (_secondaryActionLabel != null)
                _secondaryActionLabel.text = secondaryLabel;
        }

        private void OnPrimaryActionClicked() => ContinueClicked?.Invoke();

        private void OnSecondaryActionClicked() => SecondaryClicked?.Invoke();

        protected override void ModifyShowAnimation(Sequence animation)
        {
            base.ModifyShowAnimation(animation);

            foreach (Transform star in _stars)
            {
                animation
                    .Append(star.DOScale(1, 0.3f).SetEase(Ease.OutBack).From(0))
                    .Join(star.DOLocalRotate(Vector3.forward * 360, 0.3f, RotateMode.LocalAxisAdd)
                        .SetEase(Ease.OutCubic)
                        .From(Vector3.zero));
                animation.AppendInterval(0.1f);
            }
        }
    }
}
