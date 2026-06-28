using System;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Abilities;
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
        private readonly AbilitySlotView _view;

        private Entity _mainHero;
        private AbilityType _abilityType;
        private int _costInEssence;

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
            
            _costInEssence = GetCostInEssence(abilityType, configsProviderService);
        }
        
        public AbilitySlotView View => _view;
        
        private int GetCostInEssence(AbilityType type, ConfigsProviderService configsProviderService)
        {
            switch (type)
            {
                case AbilityType.PlantMine:
                    return configsProviderService.GetConfig<MineConfig>().CostInEssence;
                case AbilityType.PlantTurret:
                    return configsProviderService.GetConfig<TurretConfig>().CostInEssence;
                case AbilityType.PlantToxicArea:
                    return configsProviderService.GetConfig<ToxicAreaConfig>().CostInEssence;
                default:
                    return 0;
            }
        }

        public void Initialize()
        {
            _view.IconView.SetIcon(_abilityIconsConfig.GetSpriteFor(_abilityType));

            _view.IconTextView.SetText(_costInEssence.ToString());
            _view.IconTextView.SetIcon(_currencyIconsConfig.GetSpriteFor(CurrencyTypes.Essence));
            
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
