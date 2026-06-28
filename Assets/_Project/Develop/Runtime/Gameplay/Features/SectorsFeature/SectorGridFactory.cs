using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Sectors;
using Assets._Project.Develop.Runtime.Infrastructure.DI;
using Assets._Project.Develop.Runtime.Utilities.ConfigsManagment;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature
{
    public class SectorGridFactory
    {
        private readonly ConfigsProviderService _configsProviderService;
        private readonly SectorRegistryService _sectorRegistryService;

        public SectorGridFactory(DIContainer container)
        {
            _configsProviderService = container.Resolve<ConfigsProviderService>();
            _sectorRegistryService = container.Resolve<SectorRegistryService>();
        }

        public void Create()
        {
            TowerConfig towerConfig = _configsProviderService.GetConfig<TowerConfig>();
            SectorGridConfig gridConfig = _configsProviderService.GetConfig<SectorGridConfig>();
            SectorVisualConfig visualConfig = _configsProviderService.GetConfig<SectorVisualConfig>();

            _sectorRegistryService.InitializeGrid(GetGridCenter(towerConfig), gridConfig);

            SectorBootstrap sectorBootstrap = Object.FindObjectOfType<SectorBootstrap>();

            if (sectorBootstrap != null)
                sectorBootstrap.RegisterSceneVolumes(_sectorRegistryService, visualConfig);
        }

        private static Vector3 GetGridCenter(TowerConfig towerConfig)
        {
            SectorBootstrap sectorBootstrap = Object.FindObjectOfType<SectorBootstrap>();

            if (sectorBootstrap != null)
                return sectorBootstrap.transform.position;

            return towerConfig.StartPosition;
        }
    }
}
