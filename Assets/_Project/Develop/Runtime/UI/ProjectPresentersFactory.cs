using _Project.Develop.Runtime.Configs.Gameplay.Abilities;
using _Project.Develop.Runtime.Configs.Meta.Stats;
using _Project.Develop.Runtime.Meta.Features.Powerups;
using _Project.Develop.Runtime.UI.Gameplay.Abilities;
using Assets._Project.Develop.Runtime.Configs.Meta.Stats;
using Assets._Project.Develop.Runtime.Configs.Meta.Wallet;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.Ability;
using Assets._Project.Develop.Runtime.Infrastructure.DI;
using Assets._Project.Develop.Runtime.Meta.Features.Wallet;
using Assets._Project.Develop.Runtime.UI.CommonViews;
using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.UI.MainMenu;
using Assets._Project.Develop.Runtime.UI.MainMenu.ShopPopup;
using Assets._Project.Develop.Runtime.UI.Stats;
using Assets._Project.Develop.Runtime.UI.Wallet;
using Assets._Project.Develop.Runtime.Utilities.Audio;
using Assets._Project.Develop.Runtime.Utilities.ConfigsManagment;
using Assets._Project.Develop.Runtime.Utilities.CoroutinesManagment;
using Assets._Project.Develop.Runtime.Utilities.DataManagment.DataProviders;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using Assets._Project.Develop.Runtime.Utilities.SceneManagment;

namespace Assets._Project.Develop.Runtime.UI
{
    public class ProjectPresentersFactory
    {
        private readonly DIContainer _container;

        public ProjectPresentersFactory(DIContainer container)
        {
            _container = container;
        }

        public CurrencyPresenter CreateCurrencyPresenter(
            IconTextView view,
            IReadOnlyVariable<int> currency,
            CurrencyTypes currencyType)
        {
            return new CurrencyPresenter(
                currency,
                currencyType,
                _container.Resolve<ConfigsProviderService>().GetConfig<CurrencyIconsConfig>(),
                view);
        }
        
        public SingleGameStatPresenter CreateSingleGameStatPresenter(
            IconTextView view,
            IReadOnlyVariable<int> stat,
            StatType statType)
        {
            return new SingleGameStatPresenter(
                stat,
                statType,
                _container.Resolve<ConfigsProviderService>().GetConfig<StatIconsConfig>(),
                view);
        }
        
        public SingleAbilityPresenter CreateSingleAbilityPresenter(
            IconView view,
            AbilityType abilityType,
            Entity mainHero)
        {
            return new SingleAbilityPresenter(
                abilityType,
                _container.Resolve<ConfigsProviderService>().GetConfig<AbilityIconsConfig>(),
                view,
                mainHero);
        }

        public WalletPresenter CreateWalletPresenter(IconTextListView view)
        {
            return new WalletPresenter(
                _container.Resolve<WalletService>(),
                this,
                _container.Resolve<ViewsFactory>(),
                view);
        }

        public GameStatsPresenter CreateGameStatsPresenter(IconTextListView view)
        {
            return new GameStatsPresenter(
                _container.Resolve<StatsService>(),
                this,
                _container.Resolve<ViewsFactory>(),
                view);
        }

        public CharacterPreviewPresenter CreateCharacterPreviewPresenter()
        {
            return new CharacterPreviewPresenter(_container.Resolve<SceneLoaderService>(), _container.Resolve<ICoroutinesPerformer>());
        }
        
        public ShopPopupPresenter CreateShopPopupPresenter(ShopPopupView view)
        {
            return new ShopPopupPresenter(
                _container.Resolve<ICoroutinesPerformer>(),
                view,
                _container.Resolve<WalletService>(),
                _container.Resolve<PowerupService>(),
                _container.Resolve<PlayerDataProvider>(),
                _container.Resolve<ConfigsProviderService>(),
                _container.Resolve<IUISoundService>());
        }
    }
}
