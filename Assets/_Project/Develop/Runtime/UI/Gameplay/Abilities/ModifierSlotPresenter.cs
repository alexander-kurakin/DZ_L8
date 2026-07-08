using System;
using Assets._Project.Develop.Runtime.Configs.Gameplay.ProjectileModifiers;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.ProjectileModifiers;
using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.Utilities.ConfigsManagment;

namespace _Project.Develop.Runtime.UI.Gameplay.Abilities
{
    public class ModifierSlotPresenter : IPresenter
    {
        private readonly ProjectileModifierIconsConfig _modifierIconsConfig;
        private readonly AbilitySlotView _view;
        private readonly Entity _playerEntity;
        private readonly ModifierType _modifierType;

        private IDisposable _activeModifierDisposable;

        public ModifierSlotPresenter(
            ModifierType modifierType,
            ConfigsProviderService configsProviderService,
            AbilitySlotView view,
            Entity playerEntity)
        {
            _modifierType = modifierType;
            _view = view;
            _playerEntity = playerEntity;
            _modifierIconsConfig = configsProviderService.GetConfig<ProjectileModifierIconsConfig>();
        }

        public AbilitySlotView View => _view;

        public void Initialize()
        {
            _view.SetCostVisible(false);
            _view.IconView.SetIcon(_modifierIconsConfig.GetSpriteFor(_modifierType));
            _view.IconView.IconClicked += OnIconClicked;

            UpdateHighlight(_playerEntity.GetComponent<ActiveProjectileModifier>().Value.Value);

            _activeModifierDisposable = _playerEntity.GetComponent<ActiveProjectileModifier>().Value
                .Subscribe(OnActiveModifierChanged);
        }

        private void OnActiveModifierChanged(ModifierType oldValue, ModifierType newValue)
        {
            UpdateHighlight(newValue);
        }

        private void UpdateHighlight(ModifierType activeModifier)
        {
            _view.IconView.SetHighlighted(_modifierType == activeModifier);
        }

        private void OnIconClicked()
        {
            _playerEntity.GetComponent<ActiveProjectileModifier>().Value.Value = _modifierType;
        }

        public void Dispose()
        {
            _view.IconView.IconClicked -= OnIconClicked;
            _activeModifierDisposable?.Dispose();
        }
    }
}
