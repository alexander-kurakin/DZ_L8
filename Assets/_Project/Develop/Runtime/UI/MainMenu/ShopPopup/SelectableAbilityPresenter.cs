using System;
using _Project.Develop.Runtime.Meta.Features.Powerups;
using Assets._Project.Develop.Runtime.Configs.Meta.NewPowerups;
using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.Utilities.CoroutinesManagment;
using Assets._Project.Develop.Runtime.Utilities.DataManagment.DataProviders;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.UI.MainMenu.ShopPopup
{
    public class SelectableAbilityPresenter : IPresenter
    {
        public event Action<SelectableAbilityPresenter> Selected;
        
        private readonly PowerupService _powerupService;

        public PowerupConfig PowerupConfig { get; }
        public SelectableAbilityView View { get; }

        public SelectableAbilityPresenter(
            PowerupConfig powerupConfig,
            SelectableAbilityView view,
            PowerupService powerupService)
        {
            PowerupConfig = powerupConfig;
            View = view;
            
            _powerupService = powerupService;
        }
        
        public void Initialize()
        {
            View.SetName(PowerupConfig.Name);
            View.SetDescription(PowerupConfig.Description);
            View.Icon.SetIcon(PowerupConfig.Icon);
            
            UpdateByAbilityConfig();

            View.Clicked += OnViewClicked;
        }
        
        public void Dispose()
        {
            View.Clicked -= OnViewClicked;
        }

        public void AnimateCannotBuy() => View.CannotBuy();

        public void AnimateSuccessfulBuy() => View.SuccessfulBuy();

        public bool IsUnlocked() => _powerupService.GetPowerupDataByID(PowerupConfig.ID).Unlocked;

        public void Provide() => _powerupService.UnlockPowerup(PowerupConfig.ID);

        private void OnViewClicked() => Selected?.Invoke(this);

        public void UpdateByAbilityConfig()
        {
            View.Icon.HideLevel();

            if (_powerupService.GetPowerupDataByID(PowerupConfig.ID).Unlocked)
            {
                View.SetTabletText("OWNED");
                View.SetTabletColor(Color.green);
            }
            else
            {
                View.SetTabletText("NEW");
                View.SetTabletColor(Color.magenta);
            }
        }
    }
}