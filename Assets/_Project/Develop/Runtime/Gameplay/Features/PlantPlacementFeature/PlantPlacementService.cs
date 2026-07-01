using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.Ability;
using Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature;
using Assets._Project.Develop.Runtime.Utilities;
using System;
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

        public bool CanPlaceOnSector(SectorId sectorId, AbilityType abilityType)
        {
            return GetSectorPreviewState(sectorId, abilityType) == PlantPlacementPreviewState.Allowed;
        }

        public PlantPlacementPreviewState GetSectorPreviewState(SectorId sectorId, AbilityType abilityType)
        {
            if (_sectorRegistryService.IsInitialized == false)
                return PlantPlacementPreviewState.BlockedInPrinciple;

            if (IsPlantableBelt(sectorId.Belt) == false)
                return PlantPlacementPreviewState.BlockedInPrinciple;

            if (_sectorRegistryService.IsPathUnlocked(sectorId.Index) == false)
                return PlantPlacementPreviewState.BlockedInPrinciple;

            if (IsBeltAllowedForAbility(abilityType, sectorId.Belt) == false)
                return PlantPlacementPreviewState.BlockedInPrinciple;

            if (_occupiedSectors.Contains(sectorId))
                return PlantPlacementPreviewState.BlockedOccupied;

            return PlantPlacementPreviewState.Allowed;
        }

        public static bool IsPlantAbility(AbilityType abilityType)
        {
            return abilityType == AbilityType.PlantMine
                   || abilityType == AbilityType.PlantTurret
                   || abilityType == AbilityType.PlantToxicArea;
        }

        public event Action PlacementChanged;

        public void RegisterPlantedEntity(Entity plantEntity, SectorId sectorId)
        {
            _occupiedSectors.Add(sectorId);
            _sectorByPlantEntity[plantEntity] = sectorId;
            PlacementChanged?.Invoke();
        }

        public bool TryGetPlantAtSector(SectorId sectorId, out Entity plantEntity)
        {
            foreach (KeyValuePair<Entity, SectorId> entry in _sectorByPlantEntity)
            {
                if (entry.Value == sectorId)
                {
                    plantEntity = entry.Key;
                    return true;
                }
            }

            plantEntity = default;
            return false;
        }

        public void ClearForNewRun()
        {
            _occupiedSectors.Clear();
            _sectorByPlantEntity.Clear();
        }

        private static bool IsPlantableBelt(SectorBelt belt) => belt != SectorBelt.Spawn;

        private bool IsBeltAllowedForAbility(AbilityType abilityType, SectorBelt belt)
        {
            switch (abilityType)
            {
                case AbilityType.PlantToxicArea:
                    return belt == SectorBelt.Outer;

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
            PlacementChanged?.Invoke();
        }
    }
}
