using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using System;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Stages
{
    public static class WaveEnemyPreviewResolver
    {
        public static WaveEnemyPreviewType Resolve(EntityConfig enemyConfig)
        {
            if (enemyConfig == null)
                throw new ArgumentNullException(nameof(enemyConfig));

            if (enemyConfig is ExplodingWalkingEnemyConfig)
                return WaveEnemyPreviewType.Cat;

            if (enemyConfig is RangedShootingEnemyConfig)
                return WaveEnemyPreviewType.Tank;

            if (enemyConfig is RangedDotWalkingEnemyConfig)
                return WaveEnemyPreviewType.Dragon;

            return WaveEnemyPreviewType.Cat;
        }
    }
}
