using Assets._Project.Develop.Runtime.Configs.Gameplay.Essence;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.Ability;
using Assets._Project.Develop.Runtime.Gameplay.Features.EssenceFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.PlantPlacementFeature
{
    public class PlantSellService
    {
        private readonly PlantPlacementService _plantPlacementService;
        private readonly SectorMembershipService _sectorMembershipService;
        private readonly RunEssenceService _runEssenceService;
        private readonly EssenceConfig _essenceConfig;
        private readonly EntitiesLifeContext _entitiesLifeContext;
        private readonly PlantSellJuiceService _plantSellJuiceService;
        private readonly SpellcoreCoachToastService _spellcoreCoachToastService;

        public PlantSellService(
            PlantPlacementService plantPlacementService,
            SectorMembershipService sectorMembershipService,
            RunEssenceService runEssenceService,
            EssenceConfig essenceConfig,
            EntitiesLifeContext entitiesLifeContext,
            PlantSellJuiceService plantSellJuiceService,
            SpellcoreCoachToastService spellcoreCoachToastService)
        {
            _plantPlacementService = plantPlacementService;
            _sectorMembershipService = sectorMembershipService;
            _runEssenceService = runEssenceService;
            _essenceConfig = essenceConfig;
            _entitiesLifeContext = entitiesLifeContext;
            _plantSellJuiceService = plantSellJuiceService;
            _spellcoreCoachToastService = spellcoreCoachToastService;
        }

        public bool TrySellAtWorldPosition(Vector3 worldPosition)
        {
            SectorId sectorId = _sectorMembershipService.ResolveFromWorldPosition(worldPosition);

            if (_plantPlacementService.TryGetPlantAtSector(sectorId, out Entity plantEntity) == false)
                return false;

            int plantedEssenceCost = 0;

            if (plantEntity.TryGetPlantedEssenceCost(out int storedCost))
                plantedEssenceCost = storedCost;

            int refund = _runEssenceService.CalculateRefund(plantedEssenceCost, _essenceConfig);
            bool isMine = plantEntity.TryGetMineCollider(out _);

            if (refund > 0)
            {
                _runEssenceService.Add(refund);
                _plantSellJuiceService.SpawnRefundNumber(worldPosition, refund);
            }

            _spellcoreCoachToastService.TryShowFreeMineSoldHint(isMine, refund);

            _entitiesLifeContext.Release(plantEntity);
            return true;
        }
    }
}
