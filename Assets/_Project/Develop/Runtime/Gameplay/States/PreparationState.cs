using _Project.Develop.Runtime.Gameplay.Features.Input;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using Assets._Project.Develop.Runtime.Configs.Gameplay.MouseConfig;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.Ability;
using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.MainHero;
using Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature;
using Assets._Project.Develop.Runtime.Utilities;
using Assets._Project.Develop.Runtime.Utilities.ConfigsManagment;
using Assets._Project.Develop.Runtime.Utilities.StateMachineCore;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.States
{
    public class PreparationState : State, IUpdatableState
    {
        private readonly PreparationTriggerService _preparationTriggerService;
        private readonly ContactTriggerConfig _contactTriggerConfig;
        private readonly MainHeroHolderService _mainHeroHolderService;
        private readonly RaycastConfig _mouseRaycastConfig;
        private IMouseInputService _mouseInputService;
        private MouseRaycastService _mouseRaycastService;
        
        private Entity _mainHero;

        public PreparationState(
            PreparationTriggerService preparationTriggerService, 
            ConfigsProviderService  configsProviderService,
            MainHeroHolderService mainHeroHolderService,
            MouseRaycastService mouseRaycastService,
            MouseInput mouseInputService)
        {
            _preparationTriggerService = preparationTriggerService;
            _contactTriggerConfig = configsProviderService.GetConfig<ContactTriggerConfig>();
            _mainHeroHolderService = mainHeroHolderService;
            _mouseRaycastConfig = configsProviderService.GetConfig<RaycastConfig>();
            _mouseInputService = mouseInputService;
            _mouseRaycastService = mouseRaycastService;
        }

        public override void Enter()
        {
            base.Enter();
            
            _preparationTriggerService.Create(_contactTriggerConfig.ContactTriggerStartPosition);
            
            _mainHero = _mainHeroHolderService.MainHero;
            _mainHero.AbilityUserActiveAbility.Value = AbilityType.PlantMine;
        }

        public void Update(float deltaTime)
        {
            _preparationTriggerService.Update(deltaTime);
            
            if (MouseClickedOnFloorLayer(out Vector3 hitPoint))
                _mainHero.AbilityUserAllAbilities[_mainHero.AbilityUserActiveAbility.Value]
                    .AbilityUseRequest.Invoke(hitPoint);
        }
        
        private bool MouseClickedOnFloorLayer(out Vector3 hitPoint)
        {
            if (_mouseInputService.FireButtonPressed)
                if (_mouseRaycastService.TryGetHit(
                        _mouseInputService.PointerScreenPosition,
                        out RaycastHit hit,
                        _mouseRaycastConfig.MouseRaycastDistance,
                        Layers.FloorAndTriggerMask))
                {
                    if (hit.collider.gameObject.layer != Layers.ContactTrigger)
                    {
                        hitPoint = hit.point;
                        return true;
                    }
                }

            hitPoint = Vector3.zero;
            return false;
        }

        public override void Exit()
        {
            base.Exit();

            _preparationTriggerService.Cleanup();
        }
    }
}
