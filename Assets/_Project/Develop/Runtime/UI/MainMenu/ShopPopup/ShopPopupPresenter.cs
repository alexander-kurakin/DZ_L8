using System.Collections.Generic;
using _Project.Develop.Runtime.Meta.Features.Powerups;
using Assets._Project.Develop.Runtime.Configs.Meta.NewPowerups;
using Assets._Project.Develop.Runtime.Meta.Features.Wallet;
using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.Utilities.Audio;
using Assets._Project.Develop.Runtime.Utilities.ConfigsManagment;
using Assets._Project.Develop.Runtime.Utilities.CoroutinesManagment;
using Assets._Project.Develop.Runtime.Utilities.DataManagment.DataProviders;

namespace Assets._Project.Develop.Runtime.UI.MainMenu.ShopPopup
{
    public class ShopPopupPresenter : PopupPresenterBase
    {
        private const string Title = "SHOP";
        private const string PowerupText = "CHOOSE WISELY..";
        
        private readonly ShopPopupView _view;
        private readonly MainMenuPresentersFactory _presentersFactory;
        private readonly ViewsFactory _viewsFactory;
        private readonly WalletService _walletService;
        private readonly IUISoundService _uiSoundService;
        private readonly ConfigsProviderService _configsProviderService;
        private readonly PowerupService _powerupService;
        private readonly ICoroutinesPerformer _coroutinesPerformer;
        private readonly PlayerDataProvider _playerDataProvider;
        
        private List<SelectableAbilityPresenter> _presenters = new();
        private SelectableAbilityPresenter _selectableAbilityPresenter;
        
        public ShopPopupPresenter(
            ICoroutinesPerformer coroutinesPerformer,
            ShopPopupView view,
            MainMenuPresentersFactory presentersFactory,
            ViewsFactory viewsFactory,
            WalletService walletService,
            ConfigsProviderService configsProviderService,
            IUISoundService uiSoundService,
            PowerupService powerupService,
            PlayerDataProvider playerDataProvider
            ) : base(coroutinesPerformer, uiSoundService)
        {
            _view = view;
            _presentersFactory = presentersFactory;
            _viewsFactory = viewsFactory;
            _walletService = walletService;
            _uiSoundService = uiSoundService;
            _configsProviderService =  configsProviderService;
            _powerupService =  powerupService;
            _coroutinesPerformer = coroutinesPerformer;
            _playerDataProvider =  playerDataProvider;
        }

        protected override PopupViewBase PopupView => _view;
        
        public override void Initialize()
        {
            base.Initialize();
            
            _view.SetTitle(Title);
            _view.SetAdditionalText(PowerupText);
            _view.BuyButtonOff();

            _view.BuyButtonClicked += OnBuyButtonClicked;

            RebuildPowerups();
        }

        private void RebuildPowerups()
        {
            PowerupConfigsContainer powerupConfigsContainer = _configsProviderService.GetConfig<PowerupConfigsContainer>();
            IReadOnlyList<PowerupUIData> powerupUIData = _powerupService.GetPowerupUIData(powerupConfigsContainer.PowerupConfigs);

            foreach (PowerupUIData uiData in powerupUIData)
            {
                SelectableAbilityView selectableAbilityView = _viewsFactory.Create<SelectableAbilityView>(ViewIDs.SelectableAbilityView);
                _view.AbilityListView.Add(selectableAbilityView);
                
                SelectableAbilityPresenter selectableAbilityPresenter = _presentersFactory.CreateSelectableAbilityPresenter(
                    uiData.Config,
                    selectableAbilityView);
                
                selectableAbilityPresenter.Selected += OnPresenterSelected;
                selectableAbilityPresenter.Initialize();
                
                _presenters.Add(selectableAbilityPresenter);
            }
            
            _view.BuyButtonOff(); //default button state
        }
        
        private void OnPresenterSelected(SelectableAbilityPresenter selected)
        {
            _view.AbilityListView.Select(selected.View);    
            _selectableAbilityPresenter = selected;
            
            _uiSoundService.Play(UISoundIDs.SelectedClick);

            if (selected.IsUnlocked() == false)
                SetViewCanBuy(selected.PowerupConfig.CostInDiamonds);
            else
                SetViewAlreadyOwned();
        }

        private void SetViewCanBuy(int price)
        {
            _view.BuyButtonOn();
            _view.SetAdditionalText($"Price: {price} diamonds");
        }

        private void SetViewAlreadyOwned()
        {
            _view.BuyButtonOff();
            _view.SetAdditionalText("Already owned");
        }

        private void OnBuyButtonClicked()
        {
            int price = _selectableAbilityPresenter.PowerupConfig.CostInDiamonds;

            if (_walletService.Enough(CurrencyTypes.Diamond, price) == false)
            {
                _uiSoundService.Play(UISoundIDs.CannotBuy);
                _selectableAbilityPresenter.AnimateCannotBuy();
                return;
            }

            _walletService.Spend(CurrencyTypes.Diamond, price);
            
            _selectableAbilityPresenter.Provide();
            
            _coroutinesPerformer.StartPerform(_playerDataProvider.SaveAsync());
            
            _selectableAbilityPresenter.UpdateByAbilityConfig();
            SetViewAlreadyOwned();
            
            _uiSoundService.Play(UISoundIDs.SuccessfulBuy);
            _selectableAbilityPresenter.AnimateSuccessfulBuy();
        }

        protected override void OnPreHide()
        {
            base.OnPreHide();
            
            _view.BuyButtonClicked -= OnBuyButtonClicked;

            foreach (SelectableAbilityPresenter abilityPresenter in _presenters)
                abilityPresenter.Selected -= OnPresenterSelected;
        }
        
        public override void Dispose()
        {
            base.Dispose();

            _view.BuyButtonClicked -= OnBuyButtonClicked;

            foreach (SelectableAbilityPresenter abilityPresenter in _presenters)
            {
                abilityPresenter.Selected -= OnPresenterSelected;
                _view.AbilityListView.Remove(abilityPresenter.View);
                _viewsFactory.Release(abilityPresenter.View);
                abilityPresenter.Dispose();
            }

            _presenters.Clear();
        }


    }
}