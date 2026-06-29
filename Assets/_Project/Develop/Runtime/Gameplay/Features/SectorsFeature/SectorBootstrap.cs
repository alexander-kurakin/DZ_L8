using Assets._Project.Develop.Runtime.Configs.Gameplay.Sectors;
using System.Collections.Generic;
using System.Linq;
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
            RefreshViews(registry, visualConfig, null, false, null);
        }

        public void RefreshViews(
            SectorRegistryService registry,
            SectorVisualConfig visualConfig,
            IReadOnlyCollection<int> spawnPathIndices,
            bool restrictFillToSpawnPaths)
        {
            RefreshViews(registry, visualConfig, spawnPathIndices, restrictFillToSpawnPaths, null);
        }

        public void RefreshViews(
            SectorRegistryService registry,
            SectorVisualConfig visualConfig,
            IReadOnlyCollection<int> spawnPathIndices,
            bool restrictFillToSpawnPaths,
            IReadOnlyCollection<int> pendingUnlockRevealPathIndices)
        {
            if (visualConfig == null)
                return;

            SectorView[] views = GetComponentsInChildren<SectorView>(true);

            foreach (SectorView view in views)
                view.Apply(
                    visualConfig,
                    registry,
                    spawnPathIndices,
                    restrictFillToSpawnPaths,
                    pendingUnlockRevealPathIndices);
        }

        public void AnimatePathUnlockReveal(
            IReadOnlyCollection<int> pathIndices,
            SectorVisualConfig visualConfig,
            SectorRegistryService registry)
        {
            if (pathIndices == null || pathIndices.Count == 0)
                return;

            if (visualConfig == null)
                return;

            SectorFillVisualData lockedFill = visualConfig.LockedFill;
            SectorFillVisualData unlockedFill = visualConfig.UnlockedFill;
            SectorView[] views = GetComponentsInChildren<SectorView>(true);

            foreach (SectorView view in views)
            {
                SectorVolumeRegistrator registrator = view.GetComponent<SectorVolumeRegistrator>();

                if (registrator == null)
                    continue;

                if (pathIndices.Contains(registrator.SectorId.Index) == false)
                    continue;

                if (registrator.SectorId.Belt == SectorBelt.Spawn)
                    continue;

                float delaySeconds = GetUnlockRevealBeltOrder(registrator.SectorId.Belt) * visualConfig.BeltRevealStepSeconds;
                view.PlayUnlockReveal(lockedFill, unlockedFill, delaySeconds, visualConfig.UnlockRevealDurationSeconds);
            }
        }

        private static int GetUnlockRevealBeltOrder(SectorBelt belt)
        {
            switch (belt)
            {
                case SectorBelt.Inner:
                    return 0;

                case SectorBelt.Middle:
                    return 1;

                case SectorBelt.Outer:
                    return 2;

                case SectorBelt.Spawn:
                    return 3;

                default:
                    return 0;
            }
        }
    }
}
