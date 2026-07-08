using System;
using System.Collections.Generic;
using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.ProjectileModifiers;
using Assets._Project.Develop.Runtime.UI;
using Assets._Project.Develop.Runtime.UI.Core;

namespace _Project.Develop.Runtime.UI.Gameplay.Abilities
{
    public class ModifierListPresenter : IPresenter
    {
        private readonly ProjectPresentersFactory _presentersFactory;
        private readonly ViewsFactory _viewsFactory;
        private readonly AbilitySlotListView _view;
        private readonly Entity _playerEntity;
        private readonly MouseOverUIService _mouseOverUIService;

        private readonly List<ModifierSlotPresenter> _modifierPresenters = new();

        public ModifierListPresenter(
            ProjectPresentersFactory presentersFactory,
            ViewsFactory viewsFactory,
            AbilitySlotListView view,
            Entity playerEntity,
            MouseOverUIService mouseOverUIService)
        {
            _presentersFactory = presentersFactory;
            _viewsFactory = viewsFactory;
            _view = view;
            _playerEntity = playerEntity;
            _mouseOverUIService = mouseOverUIService;
        }

        public void Initialize()
        {
            RebuildModifierList();
            _view.gameObject.SetActive(true);
        }

        public void RebuildModifierList()
        {
            ClearModifierPresenters();

            EquippedProjectileModifiers equippedModifiers = _playerEntity.GetComponent<EquippedProjectileModifiers>();

            foreach (ModifierType modifierType in equippedModifiers.Value)
            {
                AbilitySlotView modifierView = _viewsFactory.Create<AbilitySlotView>(ViewIDs.AbilitySlot);

                _mouseOverUIService.Register(modifierView.IconView.RectTransform);
                _view.Add(modifierView);

                ModifierSlotPresenter modifierPresenter =
                    _presentersFactory.CreateModifierSlotPresenter(modifierView, modifierType, _playerEntity);

                modifierPresenter.Initialize();
                _modifierPresenters.Add(modifierPresenter);
            }
        }

        public void Dispose()
        {
            ClearModifierPresenters();
        }

        private void ClearModifierPresenters()
        {
            foreach (ModifierSlotPresenter modifierPresenter in _modifierPresenters)
            {
                _view.Remove(modifierPresenter.View);
                _viewsFactory.Release(modifierPresenter.View);
                modifierPresenter.Dispose();
                _mouseOverUIService.Unregister(modifierPresenter.View.IconView.RectTransform);
            }

            _modifierPresenters.Clear();
        }
    }
}
