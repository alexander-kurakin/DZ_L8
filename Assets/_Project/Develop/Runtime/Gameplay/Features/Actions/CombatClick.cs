using _Project.Develop.Runtime.Configs.Gameplay.MouseActions;
using _Project.Develop.Runtime.Gameplay.Features.DealAreaDamage;
using Assets._Project.Develop.Runtime.Gameplay.Features.MainHero;
using Assets._Project.Develop.Runtime.Utilities;
using Assets._Project.Develop.Runtime.Utilities.ConfigsManagment;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.Actions
{
    public class CombatClick
    {
        private readonly AreaDamageService _areaDamageService;
        private readonly MainHeroHolderService _mainHeroHolderService;

        public CombatClick(
            AreaDamageService areaDamageService, 
            MainHeroHolderService mainHeroHolderService)
        {
            _areaDamageService = areaDamageService;
            _mainHeroHolderService = mainHeroHolderService;
        }

        public void TryPerformClick(RaycastHit hit, MouseActionsConfig mouseActionsConfig)
        {
            _areaDamageService.ApplySphereDamage(
                hit.point,
                mouseActionsConfig.TowerExplosionRadius,
                mouseActionsConfig.TowerExplosionDamage,
                Layers.CharactersMask,
                _mainHeroHolderService.MainHero);
        }
    }
}