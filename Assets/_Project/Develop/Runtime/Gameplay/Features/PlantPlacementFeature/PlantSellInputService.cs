using Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.PlantPlacementFeature
{
    public class PlantSellInputService
    {
        private readonly PlantPlacementPreviewService _plantPlacementPreviewService;
        private readonly PlantSellService _plantSellService;

        public PlantSellInputService(
            PlantPlacementPreviewService plantPlacementPreviewService,
            PlantSellService plantSellService)
        {
            _plantPlacementPreviewService = plantPlacementPreviewService;
            _plantSellService = plantSellService;
        }

        public bool TryHandleSellClick(Vector3 worldPoint)
        {
            if (_plantPlacementPreviewService.HasSellHoverTarget == false)
                return false;

            if (_plantPlacementPreviewService.IsPointerOverActiveSellShovel(worldPoint) == false)
                return false;

            return _plantSellService.TrySellAtWorldPosition(worldPoint);
        }
    }
}
