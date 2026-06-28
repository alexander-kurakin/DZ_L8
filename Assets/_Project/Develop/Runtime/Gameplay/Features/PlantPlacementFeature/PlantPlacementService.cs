using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.Ability;
using Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature;
using Assets._Project.Develop.Runtime.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.PlantPlacementFeature
{
    public class PlantPlacementService
    {
        private readonly SectorRegistryService _sectorRegistryService;
        private readonly SectorMembershipService _sectorMembershipService;
        private readonly EntitiesLifeContext _entitiesLifeContext;

        private readonly HashSet<SectorId> _occupiedSectors = new();
        private readonly Dictionary<Entity, SectorId> _sectorByPlantEntity = new();

        public PlantPlacementService(
            SectorRegistryService sectorRegistryService,
            SectorMembershipService sectorMembershipService,
            EntitiesLifeContext entitiesLifeContext)
        {
            _sectorRegistryService = sectorRegistryService;
            _sectorMembershipService = sectorMembershipService;
            _entitiesLifeContext = entitiesLifeContext;
            _entitiesLifeContext.Released += OnPlantEntityReleased;
        }

        public bool TryResolvePlacement(
            Vector3 clickWorldPosition,
            AbilityType abilityType,
            out Vector3 plantAnchorPosition,
            out SectorId sectorId)
        {
            plantAnchorPosition = default;
            sectorId = default;

            if (_sectorRegistryService.IsInitialized == false)
                return false;

            sectorId = _sectorMembershipService.ResolveSectorAtClick(clickWorldPosition);

            if (IsPlantableBelt(sectorId.Belt) == false)
                return false;

            if (_sectorRegistryService.IsPathUnlocked(sectorId.Index) == false)
                return false;

            if (IsBeltAllowedForAbility(abilityType, sectorId.Belt) == false)
                return false;

            if (_occupiedSectors.Contains(sectorId))
                return false;

            plantAnchorPosition = _sectorRegistryService.GetAnchorPosition(sectorId);
            return true;
        }

        public void RegisterPlantedEntity(Entity plantEntity, SectorId sectorId)
        {
            _occupiedSectors.Add(sectorId);
            _sectorByPlantEntity[plantEntity] = sectorId;
        }

        public void ClearForNewRun()
        {
            _occupiedSectors.Clear();
            _sectorByPlantEntity.Clear();
        }

        private static bool IsPlantableBelt(SectorBelt belt) => belt != SectorBelt.Spawn;

        private static bool IsBeltAllowedForAbility(AbilityType abilityType, SectorBelt belt)
        {
            switch (abilityType)
            {
                case AbilityType.PlantToxicArea:
                    return belt == SectorBelt.Outer || belt == SectorBelt.Middle;

                case AbilityType.PlantMine:
                case AbilityType.PlantTurret:
                    return belt == SectorBelt.Outer
                           || belt == SectorBelt.Middle
                           || belt == SectorBelt.Inner;

                default:
                    return false;
            }
        }

        private void OnPlantEntityReleased(Entity plantEntity)
        {
            if (_sectorByPlantEntity.TryGetValue(plantEntity, out SectorId sectorId) == false)
                return;

            _sectorByPlantEntity.Remove(plantEntity);
            _occupiedSectors.Remove(sectorId);
        }
    }
}
