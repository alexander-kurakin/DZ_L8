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

        public SectorId ResolveSectorAtClick(Vector3 clickWorldPosition)
        {
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

        public SectorId ResolveFlyingEnemyFromWorldPosition(Vector3 worldPosition)
        {
            if (_sectorRegistryService.IsInitialized == false)
                return default;

            return WorldToSector.ResolveForFlyingEnemy(
                worldPosition,
                _sectorRegistryService.Center,
                _sectorRegistryService.GridConfig);
        }
    }
}
