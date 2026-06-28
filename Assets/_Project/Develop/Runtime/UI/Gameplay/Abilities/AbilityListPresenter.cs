using System;
using System.Collections.Generic;
using _Project.Develop.Runtime.Gameplay.Features.InputFeature;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.Ability;
using Assets._Project.Develop.Runtime.Gameplay.Features.SpellcoreProgressionFeature;
using Assets._Project.Develop.Runtime.UI;
using Assets._Project.Develop.Runtime.UI.Core;
using UnityEngine;

namespace _Project.Develop.Runtime.UI.Gameplay.Abilities
{
    public class AbilityListPresenter : IPresenter
    {
        private readonly ProjectPresentersFactory _presentersFactory;
        private readonly ViewsFactory _viewsFactory;
        private readonly SpellcoreProgressionService _spellcoreProgressionService;

        private readonly AbilitySlotListView _view;
        private readonly Entity _mainHero;
        
        private MouseOverUIService _mouseOverUIService;

        private readonly List<SingleAbilityPresenter> _abilityPresenters = new();
        
        public AbilityListPresenter(
            ProjectPresentersFactory presentersFactory,
            ViewsFactory viewsFactory,
            AbilitySlotListView view,
            Entity mainHero,
            MouseOverUIService mouseOverUIService,
            SpellcoreProgressionService spellcoreProgressionService)
        {
            _presentersFactory = presentersFactory;
            _viewsFactory = viewsFactory;
            _view = view;
            _mainHero = mainHero;
            _mouseOverUIService = mouseOverUIService;
            _spellcoreProgressionService = spellcoreProgressionService;
        }
        
        public void Initialize()
        {
            RebuildAbilityList();
            _spellcoreProgressionService.Changed += OnProgressionChanged;
        }

        private void OnProgressionChanged()
        {
            RebuildAbilityList();
        }

        public void RebuildAbilityList()
        {
            ClearAbilityPresenters();
            BuildAbilityList();
        }

        private void BuildAbilityList()
        {
            foreach (AbilityType abilityType in _mainHero.AbilityUserAllAbilities.Keys)
            {
                if (abilityType == AbilityType.ExplodeAtPoint)
                    continue;

                if (_spellcoreProgressionService.IsAbilityUnlocked(abilityType) == false)
                    continue;
                
                AbilitySlotView abilityView = _viewsFactory.Create<AbilitySlotView>(ViewIDs.AbilitySlot);
                
                _mouseOverUIService.Register(abilityView.IconView.RectTransform);
                _mouseOverUIService.Register(abilityView.IconTextView.RectTransform);

                _view.Add(abilityView);

                SingleAbilityPresenter singleAbilityPresenter =
                    _presentersFactory.CreateSingleAbilityPresenter(
                        abilityView, abilityType, _mainHero);

                singleAbilityPresenter.Initialize();
                _abilityPresenters.Add(singleAbilityPresenter);
            }
        }

        public void HideAll()
        {
            _view.gameObject.SetActive(false);
        }
        
        public void ShowAll()
        {
            _view.gameObject.SetActive(true);
        }

        public void Dispose()
        {
            _spellcoreProgressionService.Changed -= OnProgressionChanged;
            ClearAbilityPresenters();
        }

        private void ClearAbilityPresenters()
        {
            foreach (SingleAbilityPresenter singleAbilityPresenter in _abilityPresenters)
            {
                _view.Remove(singleAbilityPresenter.View);
                _viewsFactory.Release(singleAbilityPresenter.View);
                singleAbilityPresenter.Dispose();
                _mouseOverUIService.Unregister(singleAbilityPresenter.View.IconView.RectTransform);
                _mouseOverUIService.Unregister(singleAbilityPresenter.View.IconTextView.RectTransform);
            }

            _abilityPresenters.Clear();
        }
    }
}
