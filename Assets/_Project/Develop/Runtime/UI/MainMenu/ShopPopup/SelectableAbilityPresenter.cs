using System;
using _Project.Develop.Runtime.Meta.Features.Powerups;
using Assets._Project.Develop.Runtime.Configs.Meta.NewPowerups;
using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.Utilities.CoroutinesManagment;
using Assets._Project.Develop.Runtime.Utilities.DataManagment.DataProviders;

namespace Assets._Project.Develop.Runtime.UI.MainMenu.ShopPopup
{
    public class SelectableAbilityPresenter : IPresenter
    {
        public event Action<SelectableAbilityPresenter> Selected;
        
        private readonly PowerupService _powerupService;
        private readonly PlayerDataProvider _playerDataProvider;
        private readonly ICoroutinesPerformer _coroutinesPerformer;

        public PowerupConfig PowerupConfig { get; }
        public SelectableAbilityView View { get; }

        public SelectableAbilityPresenter(
            PowerupConfig powerupConfig,
            SelectableAbilityView view,
            PowerupService powerupService,
            PlayerDataProvider playerDataProvider,
            ICoroutinesPerformer coroutinesPerformer)
        {
            PowerupConfig = powerupConfig;
            View = view;
            
            _powerupService = powerupService;
            _playerDataProvider = playerDataProvider;
            _coroutinesPerformer = coroutinesPerformer;
        }
        
        public void Initialize()
        {
            View.SetName(PowerupConfig.Name);
            View.SetDescription(PowerupConfig.Description);
            View.Icon.SetIcon(PowerupConfig.Icon);
            
            //

            InitByAbilityConfig();

            View.Clicked += OnViewClicked;
        }
        
        public void Dispose()
        {
            View.Clicked -= OnViewClicked;
        }
        
        public void Provide()
        {
            _powerupService.UnlockPowerup(PowerupConfig.ID);
            _coroutinesPerformer.StartPerform(_playerDataProvider.SaveAsync());
        }

        private void OnViewClicked() => Selected?.Invoke(this);

        private void InitByAbilityConfig()
        {
            View.Icon.HideLevel();
            View.SetTabletText("NEW");
        }
    }
}