using Assets._Project.Develop.Runtime.UI.Core;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Project.Develop.Runtime.UI.QuitConfirmation
{
    public class QuitConfirmationPopupView : PopupViewBase
    {
        public event Action ConfirmClicked;
        public event Action CancelClicked;

        [SerializeField] private TMP_Text _title;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _cancelButton;

        public void SetTitle(string title) => _title.text = title;

        protected override void OnPreShow()
        {
            base.OnPreShow();

            _confirmButton.onClick.AddListener(OnConfirmButtonClicked);
            _cancelButton.onClick.AddListener(OnCancelButtonClicked);
        }

        protected override void OnPreHide()
        {
            base.OnPreHide();

            _confirmButton.onClick.RemoveListener(OnConfirmButtonClicked);
            _cancelButton.onClick.RemoveListener(OnCancelButtonClicked);
        }

        private void OnDisable()
        {
            _confirmButton.onClick.RemoveListener(OnConfirmButtonClicked);
            _cancelButton.onClick.RemoveListener(OnCancelButtonClicked);
        }

        private void OnConfirmButtonClicked() => ConfirmClicked?.Invoke();

        private void OnCancelButtonClicked() => CancelClicked?.Invoke();
    }
}
