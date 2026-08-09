using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.Utilities.Audio;
using Assets._Project.Develop.Runtime.Utilities.CoroutinesManagment;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Assets._Project.Develop.Runtime.UI.QuitConfirmation
{
    public class QuitConfirmationPopupPresenter : PopupPresenterBase
    {
        private const string TitleName = "Quit game?";

        private readonly QuitConfirmationPopupView _view;
        private readonly IUISoundService _uiSoundService;

        private bool _wasCursorVisible;

        public QuitConfirmationPopupPresenter(
            ICoroutinesPerformer coroutinesPerformer,
            QuitConfirmationPopupView view,
            IUISoundService uiSoundService) : base(coroutinesPerformer, uiSoundService)
        {
            _view = view;
            _uiSoundService = uiSoundService;
        }

        protected override PopupViewBase PopupView => _view;

        public override void Initialize()
        {
            base.Initialize();

            _view.SetTitle(TitleName);

            _view.ConfirmClicked += OnConfirmClicked;
            _view.CancelClicked += OnCancelClicked;

            _wasCursorVisible = Cursor.visible;
            Cursor.visible = true;

            _uiSoundService.Play(UISoundIDs.PopupOpen);
        }

        protected override void OnPreHide()
        {
            base.OnPreHide();

            _view.ConfirmClicked -= OnConfirmClicked;
            _view.CancelClicked -= OnCancelClicked;

            Cursor.visible = _wasCursorVisible;
        }

        public override void Dispose()
        {
            base.Dispose();

            _view.ConfirmClicked -= OnConfirmClicked;
            _view.CancelClicked -= OnCancelClicked;
        }

        private void OnConfirmClicked()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnCancelClicked()
        {
            OnCloseRequest();
        }
    }
}
