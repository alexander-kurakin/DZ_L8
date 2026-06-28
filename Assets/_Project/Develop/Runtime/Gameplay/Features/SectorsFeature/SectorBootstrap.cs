using Assets._Project.Develop.Runtime.Configs.Gameplay.Sectors;
using System.Collections.Generic;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature
{
    public class SectorBootstrap : MonoBehaviour
    {
        public void RegisterSceneVolumes(SectorRegistryService registry, SectorVisualConfig visualConfig)
        {
            if (registry == null)
                return;

            SectorVolumeRegistrator[] registrators = GetComponentsInChildren<SectorVolumeRegistrator>(true);

            foreach (SectorVolumeRegistrator registrator in registrators)
                registrator.Register(registry);

            RefreshViews(registry, visualConfig);
        }

        public void RefreshViews(
            SectorRegistryService registry,
            SectorVisualConfig visualConfig)
        {
            RefreshViews(registry, visualConfig, null, false);
        }

        public void RefreshViews(
            SectorRegistryService registry,
            SectorVisualConfig visualConfig,
            IReadOnlyCollection<int> spawnPathIndices,
            bool restrictFillToSpawnPaths)
        {
            if (visualConfig == null)
                return;

            SectorView[] views = GetComponentsInChildren<SectorView>(true);

            foreach (SectorView view in views)
                view.Apply(visualConfig, registry, spawnPathIndices, restrictFillToSpawnPaths);
        }
    }
}
