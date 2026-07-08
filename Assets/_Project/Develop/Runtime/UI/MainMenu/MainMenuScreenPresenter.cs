using Assets._Project.Develop.Runtime.Gameplay.Infrastructure;
using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.Utilities.Audio;
using Assets._Project.Develop.Runtime.Utilities.CoroutinesManagment;
using Assets._Project.Develop.Runtime.Utilities.SceneManagment;

namespace Assets._Project.Develop.Runtime.UI.MainMenu
{
    public class MainMenuScreenPresenter : IPresenter
    {
        private const int DEFAULT_LEVEL_NUMBER = 1;

        private readonly MainMenuScreenView _screen;
        private readonly ICoroutinesPerformer _coroutinesPerformer;
        private readonly SceneSwitcherService _sceneSwitcherService;
        private readonly IUISoundService _uiSoundService;

        public MainMenuScreenPresenter(
            MainMenuScreenView screen,
            ICoroutinesPerformer coroutinesPerformer,
            SceneSwitcherService sceneSwitcherService,
            IUISoundService uiSoundService)
        {
            _screen = screen;
            _coroutinesPerformer = coroutinesPerformer;
            _sceneSwitcherService = sceneSwitcherService;
            _uiSoundService = uiSoundService;
        }

        public void Initialize()
        {
            _screen.PlayButtonClicked += OnPlayButtonClicked;
        }

        public void Dispose()
        {
            _screen.PlayButtonClicked -= OnPlayButtonClicked;
        }

        private void OnPlayButtonClicked()
        {
            _uiSoundService.Play(UISoundIDs.ButtonClick);
            _coroutinesPerformer.StartPerform(
                _sceneSwitcherService.ProcessSwitchTo(Scenes.Gameplay, new GameplayInputArgs(DEFAULT_LEVEL_NUMBER)));
        }
    }
}
