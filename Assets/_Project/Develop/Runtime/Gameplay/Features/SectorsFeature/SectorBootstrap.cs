using Assets._Project.Develop.Runtime.Configs.Gameplay.Sectors;
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

            RefreshViews(registry, visualConfig, showSpawnPathPreview: false);
        }

        public void RefreshViews(
            SectorRegistryService registry,
            SectorVisualConfig visualConfig,
            bool showSpawnPathPreview)
        {
            if (visualConfig == null)
                return;

            SectorView[] views = GetComponentsInChildren<SectorView>(true);

            foreach (SectorView view in views)
                view.Apply(visualConfig, registry, showSpawnPathPreview);
        }
    }
}
