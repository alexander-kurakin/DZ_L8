using System;
using System.Collections.Generic;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.Ability;
using Assets._Project.Develop.Runtime.Gameplay.Features.MainHero;
using Assets._Project.Develop.Runtime.UI;
using Assets._Project.Develop.Runtime.UI.CommonViews;
using Assets._Project.Develop.Runtime.UI.Core;

namespace _Project.Develop.Runtime.UI.Gameplay.Abilities
{
    public class AbilityListPresenter : IPresenter
    {
        private readonly MainHeroHolderService _mainHeroHolderService;
        private readonly ProjectPresentersFactory _presentersFactory;
        private readonly ViewsFactory _viewsFactory;

        private readonly IconListView _view;

        private readonly List<SingleAbilityPresenter> _abilityPresenters = new();
        
        private IDisposable _heroCreatedDisposable;

        public AbilityListPresenter(
            MainHeroHolderService mainHeroHolderService,
            ProjectPresentersFactory presentersFactory,
            ViewsFactory viewsFactory,
            IconListView view)
        {
            _mainHeroHolderService = mainHeroHolderService;
            _presentersFactory = presentersFactory;
            _viewsFactory = viewsFactory;
            _view = view;
        }

        public void Initialize()
        {
            if (_mainHeroHolderService.MainHero != null)
                BuildAbilityList(_mainHeroHolderService.MainHero);
            else
                _heroCreatedDisposable = _mainHeroHolderService.HeroRegistred.Subscribe(OnHeroRegistered);
        }

        private void OnHeroRegistered(Entity mainHero)
        {
            BuildAbilityList(mainHero);
        }

        private void BuildAbilityList(Entity mainHero)
        {
            foreach (AbilityType abilityType in mainHero.AbilityUserAllAbilities.Keys)
            {
                if (abilityType == AbilityType.ExplodeAtPoint) //only draw abilities for plant
                    continue;
                
                IconView abilityView = _viewsFactory.Create<IconView>(ViewIDs.AbilityView);

                _view.Add(abilityView);

                SingleAbilityPresenter singleAbilityPresenter =
                    _presentersFactory.CreateSingleAbilityPresenter(
                        abilityView, abilityType);

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
            _heroCreatedDisposable?.Dispose();
            
            foreach (SingleAbilityPresenter singleAbilityPresenter in _abilityPresenters)
            {
                _view.Remove(singleAbilityPresenter.View);
                _viewsFactory.Release(singleAbilityPresenter.View);
                singleAbilityPresenter.Dispose();
            }

            _abilityPresenters.Clear();
        }
    }
}