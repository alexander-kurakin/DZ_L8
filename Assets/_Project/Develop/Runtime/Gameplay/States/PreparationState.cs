using _Project.Develop.Runtime.Gameplay.Features.Input;
using _Project.Develop.Runtime.Gameplay.Features.InputFeature;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using Assets._Project.Develop.Runtime.Configs.Gameplay.MouseConfig;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.Ability;
using Assets._Project.Develop.Runtime.Gameplay.Features.GameplayStateBridge;
using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.MainHero;
using Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature;
using Assets._Project.Develop.Runtime.Utilities;
using Assets._Project.Develop.Runtime.Utilities.Audio;
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
        private MouseOverUIService _mouseOverUIService;
        private IBackgroundMusicService _backgroundMusicService;
        
        private Entity _mainHero;
        private Entity _towerWalker;

        public PreparationState(
            PreparationTriggerService preparationTriggerService, 
            ConfigsProviderService  configsProviderService,
            MainHeroHolderService mainHeroHolderService,
            MouseRaycastService mouseRaycastService,
            MouseInput mouseInputService,
            IBackgroundMusicService backgroundMusicService,
            MouseOverUIService mouseOverUIService)
        {
            _preparationTriggerService = preparationTriggerService;
            _contactTriggerConfig = configsProviderService.GetConfig<ContactTriggerConfig>();
            _mainHeroHolderService = mainHeroHolderService;
            _mouseRaycastConfig = configsProviderService.GetConfig<RaycastConfig>();
            _mouseInputService = mouseInputService;
            _mouseRaycastService = mouseRaycastService;
            _backgroundMusicService = backgroundMusicService;
            _mouseOverUIService = mouseOverUIService;
        }

        public override void Enter()
        {
            base.Enter();
            
            _preparationTriggerService.Create(_contactTriggerConfig.ContactTriggerStartPosition);
            
            _mainHero = _mainHeroHolderService.MainHero;
            
            _mainHero.GameplayPhase.Value = GameplayStates.Preparation;
            _mainHeroHolderService.TowerWalker.GameplayPhase.Value = _mainHeroHolderService.MainHero.GameplayPhase.Value;

            _mainHero.AbilityUserActiveAbility.Value = _mainHero.AbilityUserPlantAbilityPreference.Value;
            
            _backgroundMusicService.Play(BackgroundMusicTrackIDs.Preparation);
        }

        public void Update(float deltaTime)
        {
            _preparationTriggerService.Update(deltaTime);

            if (_mouseOverUIService.IsPointerOverUI(_mouseInputService.PointerScreenPosition))
                return;
            
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
