using Assets._Project.Develop.Runtime.Configs.Gameplay.Stages;
using Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature;
using System.Collections.Generic;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature
{
    public class WaveSpawnPlanService
    {
        private readonly List<SpawnGroupPlanEntry> _groupPlans = new();
        private readonly List<int> _orderedUnlockedPathIndices = new();

        public IReadOnlyList<SpawnGroupPlanEntry> GroupPlans => _groupPlans;

        public void BuildForWave(
            ClearAllEnemiesWaveRuntimeData waveData,
            SectorRegistryService sectorRegistryService,
            int waveNumber)
        {
            _groupPlans.Clear();

            if (waveData == null)
                return;

            if (sectorRegistryService == null || sectorRegistryService.IsInitialized == false)
                return;

            IReadOnlyList<int> unlockedPathIndices = sectorRegistryService.UnlockedPathIndices;

            if (unlockedPathIndices.Count == 0)
                return;

            BuildOrderedUnlockedPathIndices(unlockedPathIndices);
            int roundRobinIndex = 0;

            foreach (RuntimeSpawnGroup spawnGroup in waveData.SpawnGroups)
            {
                int pathIndex = ResolvePathIndex(spawnGroup.PathIndex, ref roundRobinIndex, waveNumber);
                WaveEnemyPreviewType previewType = ResolveGroupThreatType(spawnGroup);
                _groupPlans.Add(new SpawnGroupPlanEntry(pathIndex, previewType));
            }
        }

        public void BuildForWave(
            ClearAllEnemiesStageConfig stageConfig,
            SectorRegistryService sectorRegistryService,
            int waveNumber)
        {
            if (stageConfig == null)
            {
                BuildForWave((ClearAllEnemiesWaveRuntimeData)null, sectorRegistryService, waveNumber);
                return;
            }

            BuildForWave(ClearAllEnemiesWaveRuntimeData.FromConfig(stageConfig), sectorRegistryService, waveNumber);
        }

        public bool TryGetPlannedPathIndexForGroup(int groupIndex, out int pathIndex)
        {
            if (groupIndex < 0 || groupIndex >= _groupPlans.Count)
            {
                pathIndex = default;
                return false;
            }

            pathIndex = _groupPlans[groupIndex].PathIndex;
            return true;
        }

        public void Clear()
        {
            _groupPlans.Clear();
            _orderedUnlockedPathIndices.Clear();
        }

        private void BuildOrderedUnlockedPathIndices(IReadOnlyList<int> unlockedPathIndices)
        {
            _orderedUnlockedPathIndices.Clear();

            for (int index = 0; index < unlockedPathIndices.Count; index++)
                _orderedUnlockedPathIndices.Add(unlockedPathIndices[index]);

            _orderedUnlockedPathIndices.Sort();
        }

        private int ResolvePathIndex(int configuredPathIndex, ref int roundRobinIndex, int waveNumber)
        {
            int pathCount = _orderedUnlockedPathIndices.Count;
            int waveRotation = waveNumber > 0 ? (waveNumber - 1) % pathCount : 0;

            if (configuredPathIndex >= 0)
            {
                int slotIndex = configuredPathIndex % pathCount;
                int listIndex = (slotIndex + waveRotation) % pathCount;
                return _orderedUnlockedPathIndices[listIndex];
            }

            int roundRobinListIndex = (roundRobinIndex + waveRotation) % pathCount;
            roundRobinIndex++;
            return _orderedUnlockedPathIndices[roundRobinListIndex];
        }

        private static WaveEnemyPreviewType ResolveGroupThreatType(RuntimeSpawnGroup spawnGroup)
        {
            WaveEnemyPreviewType threatType = WaveEnemyPreviewType.Cat;

            foreach (RuntimeEnemyItem enemyItem in spawnGroup.EnemyItems)
            {
                WaveEnemyPreviewType itemType = WaveEnemyPreviewResolver.Resolve(enemyItem.EnemyConfig);
                threatType = PickStrongerThreat(threatType, itemType);
            }

            return threatType;
        }

        private static WaveEnemyPreviewType ResolveGroupThreatType(SpawnGroupConfig spawnGroup)
        {
            WaveEnemyPreviewType threatType = WaveEnemyPreviewType.Cat;

            foreach (EnemyItemConfig enemyItem in spawnGroup.EnemyItems)
            {
                WaveEnemyPreviewType itemType = WaveEnemyPreviewResolver.Resolve(enemyItem.EnemyConfig);
                threatType = PickStrongerThreat(threatType, itemType);
            }

            return threatType;
        }

        private static WaveEnemyPreviewType PickStrongerThreat(
            WaveEnemyPreviewType currentType,
            WaveEnemyPreviewType candidateType)
        {
            return GetThreatPriority(candidateType) > GetThreatPriority(currentType)
                ? candidateType
                : currentType;
        }

        private static int GetThreatPriority(WaveEnemyPreviewType previewType)
        {
            switch (previewType)
            {
                case WaveEnemyPreviewType.Dragon:
                    return 3;

                case WaveEnemyPreviewType.Tank:
                    return 2;

                case WaveEnemyPreviewType.Cat:
                    return 1;

                default:
                    return 0;
            }
        }
    }
}
