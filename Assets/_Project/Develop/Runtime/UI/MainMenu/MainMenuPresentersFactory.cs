using _Project.Develop.Runtime.Meta.Features.Powerups;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Levels;
using Assets._Project.Develop.Runtime.Configs.Meta.NewPowerups;
using Assets._Project.Develop.Runtime.Gameplay.Features.MainHero;
using Assets._Project.Develop.Runtime.Infrastructure.DI;
using Assets._Project.Develop.Runtime.Meta.Features.Wallet;
using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.UI.MainMenu.ShopPopup;
using Assets._Project.Develop.Runtime.Utilities.Audio;
using Assets._Project.Develop.Runtime.Utilities.ConfigsManagment;
using Assets._Project.Develop.Runtime.Utilities.CoroutinesManagment;
using Assets._Project.Develop.Runtime.Utilities.DataManagment.DataProviders;
using Assets._Project.Develop.Runtime.Utilities.SceneManagment;

namespace Assets._Project.Develop.Runtime.UI.MainMenu
{
    public class MainMenuPresentersFactory
    {
        private readonly DIContainer _container;

        public MainMenuPresentersFactory(DIContainer container)
        {
            _container = container;
        }

        public MainMenuScreenPresenter CreateMainMenuScreen(MainMenuScreenView view)
        {
            return new MainMenuScreenPresenter(
                view,
                _container.Resolve<ProjectPresentersFactory>(),
                _container.Resolve<ConfigsProviderService>().GetConfig<LevelsListConfig>(),
                _container.Resolve<ICoroutinesPerformer>(),
                _container.Resolve<SceneSwitcherService>(),
                _container.Resolve<IUISoundService>(),
                _container.Resolve<MainMenuPopupService>());
        }

        public SelectableAbilityPresenter CreateSelectableAbilityPresenter(
            PowerupConfig powerupConfig,
            SelectableAbilityView view)
        {
            return new SelectableAbilityPresenter(
                powerupConfig,
                view,
                _container.Resolve<PowerupService>(),
                _container.Resolve<PlayerDataProvider>(),
                _container.Resolve<ICoroutinesPerformer>());
        }
        
        public ShopPopupPresenter CreateShopPopupPresenter(ShopPopupView view)
        {
            
            return new ShopPopupPresenter(
                _container.Resolve<ICoroutinesPerformer>(),
                view,
                _container.Resolve<MainMenuPresentersFactory>(),
                _container.Resolve<ViewsFactory>(),
                _container.Resolve<WalletService>(),
                _container.Resolve<ConfigsProviderService>(),
                _container.Resolve<IUISoundService>(),
                _container.Resolve<PowerupService>(),
                _container.Resolve<PlayerDataProvider>());
        }
    }
}
