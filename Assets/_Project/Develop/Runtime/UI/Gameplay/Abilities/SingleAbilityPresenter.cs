using System;
using _Project.Develop.Runtime.Configs.Gameplay.Abilities;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using Assets._Project.Develop.Runtime.Configs.Meta.Wallet;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.Ability;
using Assets._Project.Develop.Runtime.Meta.Features.Wallet;
using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.Utilities.ConfigsManagment;

namespace _Project.Develop.Runtime.UI.Gameplay.Abilities
{
    public class SingleAbilityPresenter : IPresenter
    {
        private readonly AbilityIconsConfig _abilityIconsConfig;
        private readonly CurrencyIconsConfig _currencyIconsConfig;
        private readonly PurchasableEntityConfig _purchasableEntityConfig;
        private readonly AbilitySlotView _view;

        private Entity _mainHero;
        private AbilityType _abilityType;
        private int _costInGold;

        private IDisposable _plantAbilityPreferenceDisposable;
        
        public SingleAbilityPresenter(
            AbilityType abilityType,
            ConfigsProviderService configsProviderService,
            AbilitySlotView view,
            Entity mainHero)
        {
            _abilityType = abilityType;
            _view = view;
            _mainHero = mainHero;
            
            _abilityIconsConfig = configsProviderService.GetConfig<AbilityIconsConfig>();
            _currencyIconsConfig =  configsProviderService.GetConfig<CurrencyIconsConfig>();
            
            _costInGold = GetCostInGold(abilityType, configsProviderService);
        }
        
        public AbilitySlotView View => _view;
        
        private int GetCostInGold(AbilityType type, ConfigsProviderService configsProviderService)
        {
            switch (type)
            {
                case AbilityType.PlantMine:
                    return configsProviderService.GetConfig<MineConfig>().CostInGold;
                case AbilityType.PlantTurret:
                    return configsProviderService.GetConfig<TurretConfig>().CostInGold;
                case AbilityType.PlantToxicArea:
                    return configsProviderService.GetConfig<ToxicAreaConfig>().CostInGold;
                default:
                    return 1;
            }
        }

        public void Initialize()
        {
            _view.IconView.SetIcon(_abilityIconsConfig.GetSpriteFor(_abilityType));

            _view.IconTextView.SetText(_costInGold.ToString());
            _view.IconTextView.SetIcon(_currencyIconsConfig.GetSpriteFor(CurrencyTypes.Gold));
            
            _view.IconView.IconClicked += OnIconClicked;
            
            if (_abilityType == _mainHero.AbilityUserPlantAbilityPreference.Value)
                _view.IconView.SetHighlighted(true);
            else
                _view.IconView.SetHighlighted(false);

            _plantAbilityPreferenceDisposable =
                _mainHero.AbilityUserPlantAbilityPreference.Subscribe(OnAbilityPreferenceChanged);
        }
        
        private void OnAbilityPreferenceChanged(AbilityType oldValue, AbilityType newValue)
        {
            _view.IconView.SetHighlighted(_abilityType == newValue);
        }

        private void OnIconClicked()
        {
            _mainHero.AbilityUserPlantAbilityPreference.Value = _abilityType;
            _mainHero.AbilityUserActiveAbility.Value = _abilityType;
        }

        public void Dispose()
        {
            _view.IconView.IconClicked -= OnIconClicked;
            _plantAbilityPreferenceDisposable?.Dispose();
        }
    }
}