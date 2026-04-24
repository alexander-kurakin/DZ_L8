using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.Utilities.CoroutinesManagment;
using Assets._Project.Develop.Runtime.Utilities.SceneManagment;
using System;
using Assets._Project.Develop.Runtime.Utilities.Audio;

namespace Assets._Project.Develop.Runtime.UI.Gameplay.ResultsPopups
{
    public class WinPopupPresenter : PopupPresenterBase
    {
        private const string TitleName = "YOU WIN!";

        private readonly WinPopupView _view;
        private readonly SceneSwitcherService _sceneSwitcher;
        private readonly ICoroutinesPerformer _coroutinesPerformer;
        private readonly IUISoundService _uiSoundService;
        private readonly IBackgroundMusicService _backgroundMusicService;

        public WinPopupPresenter(
            ICoroutinesPerformer coroutinesPerformer,
            WinPopupView view,
            SceneSwitcherService sceneSwitcher,
            IUISoundService uiSoundService,
            IBackgroundMusicService backgroundMusicService) : base(coroutinesPerformer)
        {
            _coroutinesPerformer = coroutinesPerformer;
            _view = view;
            _sceneSwitcher = sceneSwitcher;
            _uiSoundService = uiSoundService;
            _backgroundMusicService = backgroundMusicService;
        }

        protected override PopupViewBase PopupView => _view;

        public override void Initialize()
        {
            base.Initialize();

            _view.SetTitle(TitleName);

            _view.ContinueClicked += OnContinueClicked;
            
            _backgroundMusicService.Stop();
            _uiSoundService.Play(UISoundIDs.PopupOpen);
        }

        protected override void OnPreShow()
        {
            base.OnPreShow();
            
            _uiSoundService.Play(UISoundIDs.WinFanfare);
        }

        protected override void OnPreHide()
        {
            base.OnPreHide();

            _view.ContinueClicked -= OnContinueClicked;
        }

        public override void Dispose()
        {
            base.Dispose();

            _view.ContinueClicked -= OnContinueClicked;
        }

        private void OnContinueClicked()
        {
            _coroutinesPerformer.StartPerform(_sceneSwitcher.ProcessSwitchTo(Scenes.MainMenu));
            OnCloseRequest();
        }
    }
}
