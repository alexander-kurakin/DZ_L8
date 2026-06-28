using System;
using Assets._Project.Develop.Runtime.Utilities;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature
{
    public class SectorMembershipService
    {
        private const float CLICK_OVERLAP_RADIUS = 2f;

        private static readonly Collider[] OVERLAP_BUFFER = new Collider[8];

        private readonly SectorRegistryService _sectorRegistryService;

        public SectorMembershipService(SectorRegistryService sectorRegistryService)
        {
            _sectorRegistryService = sectorRegistryService;
        }

        public SectorId ResolveSectorAtClick(Vector3 clickWorldPosition)
        {
            if (_sectorRegistryService.IsInitialized == false)
                return default;

            int overlapCount = Physics.OverlapSphereNonAlloc(
                clickWorldPosition,
                CLICK_OVERLAP_RADIUS,
                OVERLAP_BUFFER,
                Layers.ContactTriggerLayerMask);

            if (overlapCount > 0)
                return Resolve(clickWorldPosition, OVERLAP_BUFFER, overlapCount);

            return ResolveFromWorldPosition(clickWorldPosition);
        }

        public SectorId ResolveFromWorldPosition(Vector3 worldPosition)
        {
            if (_sectorRegistryService.IsInitialized == false)
                return default;

            return WorldToSector.Resolve(
                worldPosition,
                _sectorRegistryService.Center,
                _sectorRegistryService.GridConfig);
        }

        public SectorId Resolve(Vector3 worldPosition, Collider[] overlappingColliders, int overlappingCount)
        {
            SectorId worldSector = ResolveFromWorldPosition(worldPosition);
            SectorId bestSector = worldSector;
            int bestIndexDelta = int.MaxValue;

            for (int index = 0; index < overlappingCount; index++)
            {
                Collider collider = overlappingColliders[index];

                if (_sectorRegistryService.TryResolveSectorFromCollider(collider, out SectorId sectorId) == false)
                    continue;

                if (sectorId.Belt != worldSector.Belt)
                    continue;

                int indexDelta = GetCircularIndexDelta(sectorId.Index, worldSector.Index);

                if (indexDelta >= bestIndexDelta)
                    continue;

                bestSector = sectorId;
                bestIndexDelta = indexDelta;
            }

            return bestSector;
        }

        private static int GetCircularIndexDelta(int firstIndex, int secondIndex)
        {
            int delta = Math.Abs(firstIndex - secondIndex);
            return Math.Min(delta, SectorId.SectorsPerRing - delta);
        }
    }
}