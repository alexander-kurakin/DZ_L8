using System;
using _Project.Develop.Runtime.Configs.Meta.Powerups;
using _Project.Develop.Runtime.Meta.Features.Powerups;
using Assets._Project.Develop.Runtime.Meta.Features.Wallet;
using Assets._Project.Develop.Runtime.UI.CommonViews;
using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.Utilities.Audio;
using Assets._Project.Develop.Runtime.Utilities.ConfigsManagment;
using Assets._Project.Develop.Runtime.Utilities.CoroutinesManagment;
using Assets._Project.Develop.Runtime.Utilities.DataManagment.DataProviders;

namespace Assets._Project.Develop.Runtime.UI.MainMenu.ShopPopup
{
    public class ShopPopupPresenter : PopupPresenterBase
    {
        private const string TitleName = "SHOP";

        private readonly ShopPopupView _view;
        private readonly ICoroutinesPerformer _coroutinesPerformer;
        private readonly WalletService _walletService;
        private readonly PowerupService _powerupService;
        private readonly PlayerDataProvider _playerDataProvider;
        private readonly PermanentPowerupsConfig  _permanentPowerupsConfig;
        private readonly IUISoundService _uiSoundService;
        
        public ShopPopupPresenter(
            ICoroutinesPerformer coroutinesPerformer,
            ShopPopupView view,
            WalletService walletService,
            PowerupService powerupService,
            PlayerDataProvider playerDataProvider,
            ConfigsProviderService configsProviderService,
            IUISoundService uiSoundService) : base(coroutinesPerformer, uiSoundService)
        {
            _view = view;
            _coroutinesPerformer = coroutinesPerformer;
            _walletService = walletService;
            _powerupService = powerupService;
            _playerDataProvider = playerDataProvider;
            _permanentPowerupsConfig = configsProviderService.GetConfig<PermanentPowerupsConfig>();
            _uiSoundService = uiSoundService;
        }

        protected override PopupViewBase PopupView => _view;
        
        public override void Initialize()
        {
            base.Initialize();

            _view.SetTitle(TitleName);

            _view.TowerHealOuterView.IconClicked += OnTowerHealthClicked;
            _view.EnemiesDebuffOuterView.IconClicked += OnEnemiesDebuffClicked;
            _view.PowerfulClickOuterView.IconClicked += OnPowerfulClickClicked;

            RefreshPowerupsData();
        }

        private void RefreshPowerupsData()
        {
            foreach (PowerupType type in Enum.GetValues(typeof(PowerupType)))
                RefreshPowerupData(type);
        }

        private void RefreshPowerupData(PowerupType type)
        {
            bool alreadyOwned = _powerupService.GetBy(type).Value;
            int price = _permanentPowerupsConfig.GetDiamondPriceBy(type);
            
            switch (type)
            {
                case PowerupType.HealExtra:
                    RefreshSingleSlot(
                        _view.TowerHealInnerTextView, 
                        _view.TowerHealOuterView, 
                        alreadyOwned, 
                        price);
                    break;
                
                case PowerupType.DamageFirstEnemies:
                    RefreshSingleSlot(
                        _view.EnemiesDebuffInnerTextView, 
                        _view.EnemiesDebuffOuterView, 
                        alreadyOwned, 
                        price);
                    break;
                
                case PowerupType.IncreaseClickDamage:
                    RefreshSingleSlot(
                        _view.PowerfulClickInnerTextView, 
                        _view.PowerfulClickOuterView, 
                        alreadyOwned, 
                        price);
                    break;
            }
        }

        private void RefreshSingleSlot (
            IconTextView priceView,
            IconView iconView,
            bool alreadyOwned,
            int price)
        {
            if (alreadyOwned)
            {
                priceView.SetText("ALREADY OWNED");
                iconView.SetHighlighted(true);
            }
            else
            {
                priceView.SetText(price.ToString());
                iconView.SetHighlighted(false);
            }
        }
        
        private void TryBuy(PowerupType type)
        {
            if (_powerupService.GetBy(type).Value)
                return;
            
            int price = _permanentPowerupsConfig.GetDiamondPriceBy(type);
            
            if (price <= 0)
                return;
            
            if (_walletService.Enough(CurrencyTypes.Diamond, price) == false)
                return;
            
            _walletService.Spend(CurrencyTypes.Diamond, price);
            _powerupService.Set(type, true);
            _coroutinesPerformer.StartPerform(_playerDataProvider.SaveAsync());
            
            _uiSoundService.Play(UISoundIDs.PopupOpen);
            
            RefreshPowerupData(type);
        }

        private void OnPowerfulClickClicked() => TryBuy(PowerupType.IncreaseClickDamage);

        private void OnEnemiesDebuffClicked() => TryBuy(PowerupType.DamageFirstEnemies);

        private void OnTowerHealthClicked() => TryBuy(PowerupType.HealExtra);
        
        protected override void OnPreHide()
        {
            _view.TowerHealOuterView.IconClicked -= OnTowerHealthClicked;
            _view.EnemiesDebuffOuterView.IconClicked -= OnEnemiesDebuffClicked;
            _view.PowerfulClickOuterView.IconClicked -= OnPowerfulClickClicked;
            
            base.OnPreHide();
        }
    }
}