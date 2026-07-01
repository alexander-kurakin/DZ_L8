using System.Collections.Generic;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Stages;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature
{
    public class SurvivalWaveScalingService
    {
        public const int TIER_WAVE_INTERVAL = 5;

        private const float ENEMY_COUNT_BONUS_PER_TIER = 0.3f;
        private const float SPAWN_INTERVAL_SCALE_PER_TIER = 0.9f;
        private const float GROUP_PAUSE_SCALE_PER_TIER = 0.85f;
        private const float MIN_SPAWN_INTERVAL_SECONDS = 0.25f;
        private const float MIN_GROUP_PAUSE_SECONDS = 0.5f;

        public int GetSurvivalTierForWave(int waveNumber, int normalStagesCount)
        {
            if (waveNumber <= normalStagesCount)
                return 0;

            return (waveNumber - normalStagesCount - 1) / TIER_WAVE_INTERVAL;
        }

        public static bool IsSurvivalMilestoneCompletedWave(int completedWaves, int normalStagesCount)
        {
            if (completedWaves <= normalStagesCount)
                return false;

            return completedWaves % TIER_WAVE_INTERVAL == 0;
        }

        public ClearAllEnemiesWaveRuntimeData CreateScaledWaveData(
            ClearAllEnemiesStageConfig template,
            int survivalTier)
        {
            if (template == null)
                return null;

            List<RuntimeSpawnGroup> scaledGroups = new List<RuntimeSpawnGroup>();

            foreach (SpawnGroupConfig spawnGroup in template.SpawnGroups)
            {
                RuntimeSpawnGroup runtimeGroup = RuntimeSpawnGroup.FromConfig(spawnGroup);
                runtimeGroup.PathIndex = SpawnGroupConfig.UNSET_PATH_INDEX;

                if (survivalTier > 0)
                    ApplyTierScaling(runtimeGroup, survivalTier);

                scaledGroups.Add(runtimeGroup);
            }

            return new ClearAllEnemiesWaveRuntimeData(scaledGroups, template.EnemySpawnRadius);
        }

        private static void ApplyTierScaling(RuntimeSpawnGroup spawnGroup, int survivalTier)
        {
            float enemyCountMultiplier = 1f + survivalTier * ENEMY_COUNT_BONUS_PER_TIER;
            float spawnIntervalMultiplier = Mathf.Pow(SPAWN_INTERVAL_SCALE_PER_TIER, survivalTier);
            float groupPauseMultiplier = Mathf.Pow(GROUP_PAUSE_SCALE_PER_TIER, survivalTier);

            foreach (RuntimeEnemyItem enemyItem in spawnGroup.EnemyItems)
            {
                int scaledCount = Mathf.Max(1, Mathf.CeilToInt(enemyItem.EnemiesCount * enemyCountMultiplier));
                enemyItem.EnemiesCount = scaledCount;
            }

            spawnGroup.MinTimeBetweenSpawns = ScaleSpawnInterval(spawnGroup.MinTimeBetweenSpawns, spawnIntervalMultiplier);
            spawnGroup.MaxTimeBetweenSpawns = ScaleSpawnInterval(spawnGroup.MaxTimeBetweenSpawns, spawnIntervalMultiplier);
            spawnGroup.PauseAfterGroup = ScaleGroupPause(spawnGroup.PauseAfterGroup, groupPauseMultiplier);
        }

        private static float ScaleSpawnInterval(float intervalSeconds, float multiplier)
        {
            if (intervalSeconds <= 0f)
                return intervalSeconds;

            return Mathf.Max(MIN_SPAWN_INTERVAL_SECONDS, intervalSeconds * multiplier);
        }

        private static float ScaleGroupPause(float pauseSeconds, float multiplier)
        {
            if (pauseSeconds <= 0f)
                return pauseSeconds;

            return Mathf.Max(MIN_GROUP_PAUSE_SECONDS, pauseSeconds * multiplier);
        }
    }
}
