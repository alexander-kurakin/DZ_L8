using System.Collections.Generic;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.MainHero;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.AI.States
{
    public class MainHeroTargetSelector : ITargetSelector
    {
        private readonly MainHeroHolderService _mainHeroHolder;
        
        public MainHeroTargetSelector(MainHeroHolderService mainHeroHolder)
        {
            _mainHeroHolder = mainHeroHolder;
        }
        
        public Entity SelectTargetFrom(IEnumerable<Entity> targets)
        {
            return _mainHeroHolder.MainHero;
        }
    }
}