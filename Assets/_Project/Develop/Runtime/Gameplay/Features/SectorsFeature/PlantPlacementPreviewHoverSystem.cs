using _Project.Develop.Runtime.Gameplay.Features.Input;
using _Project.Develop.Runtime.Gameplay.Features.InputFeature;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Gameplay.Features.Ability;
using Assets._Project.Develop.Runtime.Gameplay.Features.GameplayStateBridge;
using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.PlantPlacementFeature;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature
{
    public class PlantPlacementPreviewHoverSystem : IInitializableSystem, IUpdatableSystem
    {
        private readonly IMouseInputService _mouseInputService;
        private readonly IMouseRaycastService _mouseRaycastService;
        private readonly MouseOverUIService _mouseOverUIService;
        private readonly SectorRegistryService _sectorRegistryService;
        private readonly SectorMembershipService _sectorMembershipService;
        private readonly PlantPlacementService _plantPlacementService;
        private readonly PlantPlacementPreviewService _plantPlacementPreviewService;

        private ReactiveVariable<GameplayStates> _gameplayPhase;
        private ReactiveVariable<AbilityType> _activeAbility;

        public PlantPlacementPreviewHoverSystem(
            IMouseInputService mouseInputService,
            IMouseRaycastService mouseRaycastService,
            MouseOverUIService mouseOverUIService,
            SectorRegistryService sectorRegistryService,
            SectorMembershipService sectorMembershipService,
            PlantPlacementService plantPlacementService,
            PlantPlacementPreviewService plantPlacementPreviewService)
        {
            _mouseInputService = mouseInputService;
            _mouseRaycastService = mouseRaycastService;
            _mouseOverUIService = mouseOverUIService;
            _sectorRegistryService = sectorRegistryService;
            _sectorMembershipService = sectorMembershipService;
            _plantPlacementService = plantPlacementService;
            _plantPlacementPreviewService = plantPlacementPreviewService;
        }

        public void OnInit(Entity entity)
        {
            _gameplayPhase = entity.GameplayPhase;
            _activeAbility = entity.AbilityUserActiveAbility;
        }

        public void OnUpdate(float deltaTime)
        {
            if (IsSellOrPlantPreviewPhase() == false)
            {
                _plantPlacementPreviewService.ClearHoverCross();
                _plantPlacementPreviewService.ClearHoverSellShovel();
                return;
            }

            if (_mouseOverUIService.IsPointerOverUI(_mouseInputService.PointerScreenPosition))
            {
                _plantPlacementPreviewService.ClearHoverCross();
                _plantPlacementPreviewService.ClearHoverSellShovel();
                return;
            }

            if (SectorSurfaceClickUtility.TryGetArenaPlanePoint(
                    _mouseRaycastService,
                    _mouseInputService.PointerScreenPosition,
                    _sectorRegistryService,
                    out Vector3 planePoint) == false)
            {
                _plantPlacementPreviewService.ClearHoverCross();
                _plantPlacementPreviewService.ClearHoverSellShovel();
                return;
            }

            SectorId hoveredSectorId = _sectorMembershipService.ResolveFromWorldPosition(planePoint);

            _plantPlacementPreviewService.UpdateHoverSellShovel(
                hoveredSectorId,
                _sectorRegistryService,
                _plantPlacementService);
            _plantPlacementPreviewService.UpdateActiveSellShovelPointerHighlight(planePoint);

            if (PlantPlacementService.IsPlantAbility(_activeAbility.Value))
            {
                _plantPlacementPreviewService.UpdateHoverCross(
                    hoveredSectorId,
                    _sectorRegistryService,
                    _plantPlacementService,
                    _activeAbility.Value);
            }
            else
            {
                _plantPlacementPreviewService.ClearHoverCross();
            }
        }

        private bool IsSellOrPlantPreviewPhase()
        {
            return _gameplayPhase.Value == GameplayStates.Preparation
                   || _gameplayPhase.Value == GameplayStates.StageProcess;
        }
    }
}
