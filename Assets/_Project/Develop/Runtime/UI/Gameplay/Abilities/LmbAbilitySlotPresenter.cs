using System;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Abilities;
using Assets._Project.Develop.Runtime.Configs.Meta.Wallet;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.Ability;
using Assets._Project.Develop.Runtime.Gameplay.Features.PlantBuildingBuff;
using Assets._Project.Develop.Runtime.Meta.Features.Wallet;
using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.Utilities.ConfigsManagment;

namespace _Project.Develop.Runtime.UI.Gameplay.Abilities
{
    public class LmbAbilitySlotPresenter : IPresenter
    {
        private readonly AbilityIconsConfig _abilityIconsConfig;
        private readonly CurrencyIconsConfig _currencyIconsConfig;
        private readonly PlantBuildingBuffService _plantBuildingBuffService;
        private readonly AbilitySlotView _view;
        private readonly Entity _mainHero;

        private IDisposable _activeAbilityDisposable;

        public LmbAbilitySlotPresenter(
            AbilityIconsConfig abilityIconsConfig,
            CurrencyIconsConfig currencyIconsConfig,
            PlantBuildingBuffService plantBuildingBuffService,
            AbilitySlotView view,
            Entity mainHero)
        {
            _abilityIconsConfig = abilityIconsConfig;
            _currencyIconsConfig = currencyIconsConfig;
            _plantBuildingBuffService = plantBuildingBuffService;
            _view = view;
            _mainHero = mainHero;
        }

        public AbilitySlotView View => _view;

        public void Initialize()
        {
            _view.IconView.SetIcon(_abilityIconsConfig.GetSpriteFor(AbilityType.LeftClickAtPoint));
            _view.SetCostVisible(true);
            _view.SetCountVisible(true);

            int buffEssenceCost = _plantBuildingBuffService.GetBuffEssenceCost();
            _view.IconTextView.SetText(buffEssenceCost.ToString());
            _view.IconTextView.SetIcon(_currencyIconsConfig.GetSpriteFor(CurrencyTypes.Essence));

            UpdateBuffCountText();

            _view.IconView.IconClicked += OnIconClicked;
            UpdateHighlight(_mainHero.AbilityUserActiveAbility.Value);

            _activeAbilityDisposable =
                _mainHero.AbilityUserActiveAbility.Subscribe(OnActiveAbilityChanged);

            _plantBuildingBuffService.BuffTimerStarted += OnBuffCountChanged;
            _plantBuildingBuffService.BuffTimerEnded += OnBuffCountChanged;
        }

        private void OnIconClicked()
        {
            _mainHero.AbilityUserActiveAbility.Value = AbilityType.LeftClickAtPoint;
        }

        private void OnActiveAbilityChanged(AbilityType oldValue, AbilityType newValue)
        {
            UpdateHighlight(newValue);
        }

        private void OnBuffCountChanged(Entity plantEntity)
        {
            UpdateBuffCountText();
        }

        private void UpdateBuffCountText()
        {
            int current = _plantBuildingBuffService.ActiveBuffCount;
            int max = _plantBuildingBuffService.MaxActiveBuffCount;
            string countText = $"{current}/{max}";
            _view.SetCountText(countText);
        }

        private void UpdateHighlight(AbilityType activeAbility)
        {
            _view.IconView.SetHighlighted(activeAbility == AbilityType.LeftClickAtPoint);
        }

        public void Dispose()
        {
            _view.IconView.IconClicked -= OnIconClicked;
            _activeAbilityDisposable?.Dispose();
            _plantBuildingBuffService.BuffTimerStarted -= OnBuffCountChanged;
            _plantBuildingBuffService.BuffTimerEnded -= OnBuffCountChanged;
        }
    }
}
