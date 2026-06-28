using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature
{
    public class SectorMembershipService
    {
        private readonly SectorRegistryService _sectorRegistryService;

        public SectorMembershipService(SectorRegistryService sectorRegistryService)
        {
            _sectorRegistryService = sectorRegistryService;
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
            SectorId bestSector = ResolveFromWorldPosition(worldPosition);
            int bestPriority = bestSector.BeltPriority;

            for (int index = 0; index < overlappingCount; index++)
            {
                Collider collider = overlappingColliders[index];

                if (_sectorRegistryService.TryResolveSectorFromCollider(collider, out SectorId sectorId) == false)
                    continue;

                if (sectorId.BeltPriority <= bestPriority)
                    continue;

                bestSector = sectorId;
                bestPriority = sectorId.BeltPriority;
            }

            return bestSector;
        }
    }
}
