using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.UI.Wallet;
using System.Collections.Generic;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Levels;
using Assets._Project.Develop.Runtime.Gameplay.Infrastructure;
using Assets._Project.Develop.Runtime.UI.Stats;
using Assets._Project.Develop.Runtime.Utilities.Audio;
using Assets._Project.Develop.Runtime.Utilities.CoroutinesManagment;
using Assets._Project.Develop.Runtime.Utilities.SceneManagment;

namespace Assets._Project.Develop.Runtime.UI.MainMenu
{
    public class MainMenuScreenPresenter : IPresenter
    {
        private readonly MainMenuScreenView _screen;

        private readonly ProjectPresentersFactory _projectPresentersFactory;

        private readonly List<IPresenter> _childPresenters = new();
        
        private readonly LevelsListConfig _levelsListConfig;
        
        private readonly ICoroutinesPerformer _coroutinesPerformer;
        private readonly SceneSwitcherService _sceneSwitcherService;
        private readonly IUISoundService _uiSoundService;
        private readonly MainMenuPopupService _popupService;
        
        public MainMenuScreenPresenter(
            MainMenuScreenView screen,
            ProjectPresentersFactory projectPresentersFactory,
            LevelsListConfig levelsListConfig,
            ICoroutinesPerformer coroutinesPerformer,
            SceneSwitcherService sceneSwitcherService,
            IUISoundService uiSoundService,
            MainMenuPopupService popupService
            )
        {
            _screen = screen;
            _projectPresentersFactory = projectPresentersFactory;
            _levelsListConfig = levelsListConfig;
            _coroutinesPerformer = coroutinesPerformer;
            _sceneSwitcherService = sceneSwitcherService;
            _uiSoundService = uiSoundService;
            _popupService = popupService;
        }

        public void Initialize()
        {
            _screen.PlayButtonClicked += OnPlayButtonClicked;
            _screen.ShopButtonClicked += OnShopButtonClicked;

            CreateWallet();
            CreateStats();
            CreateCharacterPreview();

            foreach (IPresenter presenter in _childPresenters)
                presenter.Initialize();
        }

        private void OnShopButtonClicked()
        {
            _uiSoundService.Play(UISoundIDs.ButtonClick);

            _popupService.OpenShopPopup();
        }

        public void Dispose()
        {
            _screen.PlayButtonClicked -= OnPlayButtonClicked;

            foreach (IPresenter presenter in _childPresenters)
                presenter.Dispose();

            _childPresenters.Clear();
        }

        private void CreateWallet()
        {
            WalletPresenter walletPresenter = _projectPresentersFactory.CreateWalletPresenter(_screen.WalletIconTextListView);
            _childPresenters.Add(walletPresenter);
        }

        private void CreateStats()
        {
            GameStatsPresenter gameStatsPresenter = _projectPresentersFactory.CreateGameStatsPresenter(_screen.StatsIconTextListView);
            _childPresenters.Add(gameStatsPresenter);            
        }

        private void CreateCharacterPreview()
        {
            CharacterPreviewPresenter characterPreviewPresenter = _projectPresentersFactory.CreateCharacterPreviewPresenter();
            _childPresenters.Add(characterPreviewPresenter);
        }

        private void OnPlayButtonClicked()
        {
            _uiSoundService.Play(UISoundIDs.ButtonClick);
            
            int randomLevel = _levelsListConfig.GetRandomLevelNumber();
            _coroutinesPerformer.StartPerform(_sceneSwitcherService.ProcessSwitchTo(Scenes.Gameplay, new GameplayInputArgs(randomLevel)));
        }
    }
}
