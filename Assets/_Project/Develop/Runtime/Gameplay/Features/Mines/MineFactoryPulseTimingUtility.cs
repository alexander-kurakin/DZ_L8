using Assets._Project.Develop.Runtime.Configs.Gameplay.Sectors;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Spellcore;
using Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Mines
{
    public static class MineFactoryPulseTimingUtility
    {
        private const float MIN_ENEMY_MOVE_SPEED = 0.1f;

        public static float ResolvePulseProgressThreshold(
            int pulseIndex,
            int pulsesPerCrossing,
            float lastPulseProgressThreshold)
        {
            if (pulsesPerCrossing <= 0)
                return 1f;

            if (pulseIndex >= pulsesPerCrossing - 1)
                return lastPulseProgressThreshold;

            return (pulseIndex + 1) / (float)pulsesPerCrossing;
        }

        public static float ResolvePulseTimeThresholdSeconds(
            int pulseIndex,
            int pulsesPerCrossing,
            float sectorCrossSeconds,
            float lastPulseTimeFraction)
        {
            if (pulsesPerCrossing <= 0)
                return sectorCrossSeconds;

            if (pulseIndex >= pulsesPerCrossing - 1)
                return sectorCrossSeconds * lastPulseTimeFraction;

            return sectorCrossSeconds * (pulseIndex + 1) / pulsesPerCrossing;
        }

        public static float ResolveSectorCrossSeconds(
            SectorBelt belt,
            SectorGridConfig sectorGridConfig,
            float enemyMoveSpeed)
        {
            float outerRadius = WorldToSector.GetBeltOuterRadius(belt, sectorGridConfig);
            float innerRadius = WorldToSector.GetBeltInnerRadius(belt, sectorGridConfig);
            float depth = outerRadius - innerRadius;

            if (depth <= 0f)
                return 0f;

            float speed = enemyMoveSpeed;

            if (speed < MIN_ENEMY_MOVE_SPEED)
                speed = MIN_ENEMY_MOVE_SPEED;

            return depth / speed;
        }

        public static float ResolveStopBeltThirdPulseDelay(
            float sectorCrossSeconds,
            int pulsesPerCrossing,
            float lastPulseTimeFraction)
        {
            if (pulsesPerCrossing <= 0)
                return sectorCrossSeconds;

            return sectorCrossSeconds * lastPulseTimeFraction / pulsesPerCrossing;
        }

        public static float ResolveFallbackEnemyMoveSpeed(SpellcoreCombatConfig spellcoreCombatConfig)
        {
            return spellcoreCombatConfig.ReferenceCatMoveSpeed * spellcoreCombatConfig.EnemyMoveSpeedScale;
        }
    }
}
