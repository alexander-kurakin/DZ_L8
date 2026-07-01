using System.Collections.Generic;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Stages;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature
{
    public sealed class ClearAllEnemiesWaveRuntimeData
    {
        private readonly List<RuntimeSpawnGroup> _spawnGroups;

        public ClearAllEnemiesWaveRuntimeData(List<RuntimeSpawnGroup> spawnGroups, float enemySpawnRadius)
        {
            _spawnGroups = spawnGroups;
            EnemySpawnRadius = enemySpawnRadius;
        }

        public IReadOnlyList<RuntimeSpawnGroup> SpawnGroups => _spawnGroups;

        public float EnemySpawnRadius { get; }

        public int TotalEnemiesCount
        {
            get
            {
                int count = 0;

                foreach (RuntimeSpawnGroup group in _spawnGroups)
                {
                    foreach (RuntimeEnemyItem enemyItem in group.EnemyItems)
                        count += enemyItem.EnemiesCount;
                }

                return count;
            }
        }

        public static ClearAllEnemiesWaveRuntimeData FromConfig(ClearAllEnemiesStageConfig config)
        {
            List<RuntimeSpawnGroup> spawnGroups = new List<RuntimeSpawnGroup>();

            foreach (SpawnGroupConfig spawnGroup in config.SpawnGroups)
                spawnGroups.Add(RuntimeSpawnGroup.FromConfig(spawnGroup));

            return new ClearAllEnemiesWaveRuntimeData(spawnGroups, config.EnemySpawnRadius);
        }
    }

    public sealed class RuntimeSpawnGroup
    {
        public List<RuntimeEnemyItem> EnemyItems = new List<RuntimeEnemyItem>();
        public float MinTimeBetweenSpawns;
        public float MaxTimeBetweenSpawns;
        public float PauseAfterGroup;
        public int PathIndex = SpawnGroupConfig.UNSET_PATH_INDEX;

        public static RuntimeSpawnGroup FromConfig(SpawnGroupConfig source)
        {
            RuntimeSpawnGroup runtimeGroup = new RuntimeSpawnGroup
            {
                MinTimeBetweenSpawns = source.MinTimeBetweenSpawns,
                MaxTimeBetweenSpawns = source.MaxTimeBetweenSpawns,
                PauseAfterGroup = source.PauseAfterGroup,
                PathIndex = source.PathIndex
            };

            foreach (EnemyItemConfig enemyItem in source.EnemyItems)
            {
                runtimeGroup.EnemyItems.Add(new RuntimeEnemyItem
                {
                    EnemyConfig = enemyItem.EnemyConfig,
                    EnemiesCount = enemyItem.EnemiesCount
                });
            }

            return runtimeGroup;
        }

        public RuntimeSpawnGroup Clone()
        {
            RuntimeSpawnGroup clone = new RuntimeSpawnGroup
            {
                MinTimeBetweenSpawns = MinTimeBetweenSpawns,
                MaxTimeBetweenSpawns = MaxTimeBetweenSpawns,
                PauseAfterGroup = PauseAfterGroup,
                PathIndex = PathIndex
            };

            foreach (RuntimeEnemyItem enemyItem in EnemyItems)
            {
                clone.EnemyItems.Add(new RuntimeEnemyItem
                {
                    EnemyConfig = enemyItem.EnemyConfig,
                    EnemiesCount = enemyItem.EnemiesCount
                });
            }

            return clone;
        }
    }

    public sealed class RuntimeEnemyItem
    {
        public EntityConfig EnemyConfig;
        public int EnemiesCount;
    }
}
