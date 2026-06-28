using Assets._Project.Develop.Runtime.Configs.Gameplay.Stages;
using Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature
{
    public class WaveSpawnPlanService
    {
        private readonly List<SpawnGroupPlanEntry> _groupPlans = new();
        private readonly List<int> _shuffledPathIndices = new();

        public IReadOnlyList<SpawnGroupPlanEntry> GroupPlans => _groupPlans;

        public void BuildForWave(ClearAllEnemiesStageConfig stageConfig, SectorRegistryService sectorRegistryService)
        {
            _groupPlans.Clear();

            if (stageConfig == null)
                return;

            if (sectorRegistryService == null || sectorRegistryService.IsInitialized == false)
                return;

            IReadOnlyList<int> unlockedPathIndices = sectorRegistryService.UnlockedPathIndices;

            if (unlockedPathIndices.Count == 0)
                return;

            BuildShuffledPathIndices(unlockedPathIndices);

            int groupIndex = 0;

            foreach (SpawnGroupConfig spawnGroup in stageConfig.SpawnGroups)
            {
                int pathIndex = _shuffledPathIndices[groupIndex % _shuffledPathIndices.Count];
                WaveEnemyPreviewType previewType = ResolveGroupThreatType(spawnGroup);
                _groupPlans.Add(new SpawnGroupPlanEntry(pathIndex, previewType));
                groupIndex++;
            }
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
            _shuffledPathIndices.Clear();
        }

        private void BuildShuffledPathIndices(IReadOnlyList<int> unlockedPathIndices)
        {
            _shuffledPathIndices.Clear();

            for (int index = 0; index < unlockedPathIndices.Count; index++)
                _shuffledPathIndices.Add(unlockedPathIndices[index]);

            for (int index = _shuffledPathIndices.Count - 1; index > 0; index--)
            {
                int swapIndex = Random.Range(0, index + 1);
                int temporary = _shuffledPathIndices[index];
                _shuffledPathIndices[index] = _shuffledPathIndices[swapIndex];
                _shuffledPathIndices[swapIndex] = temporary;
            }
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
