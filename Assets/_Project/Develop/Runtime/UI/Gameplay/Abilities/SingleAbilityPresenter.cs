using System;
using _Project.Develop.Runtime.Configs.Gameplay.Abilities;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.Ability;
using Assets._Project.Develop.Runtime.UI.CommonViews;
using Assets._Project.Develop.Runtime.UI.Core;

namespace _Project.Develop.Runtime.UI.Gameplay.Abilities
{
    public class SingleAbilityPresenter : IPresenter
    {
        private readonly AbilityIconsConfig _abilityIconsConfig; 
        private readonly IconView _view;

        private Entity _mainHero;
        private AbilityType _abilityType;

        private IDisposable _plantAbilityPreferenceDisposable;
        
        public SingleAbilityPresenter(
            AbilityType abilityType,
            AbilityIconsConfig abilityIconsConfig,
            IconView view,
            Entity mainHero)
        {
            _abilityType = abilityType;
            _abilityIconsConfig = abilityIconsConfig;
            _view = view;
            _mainHero = mainHero;

        }
        
        public IconView View => _view;

        public void Initialize()
        {
            _view.SetIcon(_abilityIconsConfig.GetSpriteFor(_abilityType));
            
            _view.IconClicked += OnIconClicked;
            
            if (_abilityType == _mainHero.AbilityUserPlantAbilityPreference.Value)
                _view.SetHighlighted(true);
            else
                _view.SetHighlighted(false);

            _plantAbilityPreferenceDisposable =
                _mainHero.AbilityUserPlantAbilityPreference.Subscribe(OnAbilityPreferenceChanged);
        }

        private void OnAbilityPreferenceChanged(AbilityType oldValue, AbilityType newValue)
        {
            _view.SetHighlighted(_abilityType == newValue);
        }

        private void OnIconClicked()
        {
            _mainHero.AbilityUserPlantAbilityPreference.Value = _abilityType;
            _mainHero.AbilityUserActiveAbility.Value = _abilityType;
        }

        public void Dispose()
        {
            _view.IconClicked -= OnIconClicked;
            _plantAbilityPreferenceDisposable?.Dispose();
        }
    }
}