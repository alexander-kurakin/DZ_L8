using System;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Abilities;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.Ability;
using Assets._Project.Develop.Runtime.UI.Core;

namespace _Project.Develop.Runtime.UI.Gameplay.Abilities
{
    public class LmbAbilitySlotPresenter : IPresenter
    {
        private readonly AbilityIconsConfig _abilityIconsConfig;
        private readonly AbilitySlotView _view;
        private readonly Entity _mainHero;

        private IDisposable _activeAbilityDisposable;

        public LmbAbilitySlotPresenter(
            AbilityIconsConfig abilityIconsConfig,
            AbilitySlotView view,
            Entity mainHero)
        {
            _abilityIconsConfig = abilityIconsConfig;
            _view = view;
            _mainHero = mainHero;
        }

        public AbilitySlotView View => _view;

        public void Initialize()
        {
            _view.IconView.SetIcon(_abilityIconsConfig.GetSpriteFor(AbilityType.ExplodeAtPoint));
            _view.SetCostVisible(false);
            _view.IconView.IconClicked += OnIconClicked;
            UpdateHighlight(_mainHero.AbilityUserActiveAbility.Value);

            _activeAbilityDisposable =
                _mainHero.AbilityUserActiveAbility.Subscribe(OnActiveAbilityChanged);
        }

        private void OnIconClicked()
        {
            _mainHero.AbilityUserActiveAbility.Value = AbilityType.ExplodeAtPoint;
        }

        private void OnActiveAbilityChanged(AbilityType oldValue, AbilityType newValue)
        {
            UpdateHighlight(newValue);
        }

        private void UpdateHighlight(AbilityType activeAbility)
        {
            _view.IconView.SetHighlighted(activeAbility == AbilityType.ExplodeAtPoint);
        }

        public void Dispose()
        {
            _view.IconView.IconClicked -= OnIconClicked;
            _activeAbilityDisposable?.Dispose();
        }
    }
}
