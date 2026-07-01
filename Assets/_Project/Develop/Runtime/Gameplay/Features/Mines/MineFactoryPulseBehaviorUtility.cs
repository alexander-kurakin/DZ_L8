using Assets._Project.Develop.Runtime.Configs.Gameplay.Stages;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Mines
{
    public static class MineFactoryPulseBehaviorUtility
    {
        public static bool UsesStopBeltMinePattern(Entity enemy, SectorBelt mineBelt)
        {
            if (mineBelt == SectorBelt.Middle && IsTank(enemy))
                return true;

            if (mineBelt == SectorBelt.Inner && IsCat(enemy))
                return true;

            return false;
        }

        public static bool ShouldIncludeMineCellProximity(SectorBelt mineBelt)
        {
            return mineBelt == SectorBelt.Middle || mineBelt == SectorBelt.Inner;
        }

        private static bool IsTank(Entity enemy)
        {
            if (enemy.TryGetEnemyWavePreviewType(out WaveEnemyPreviewType previewType) == false)
                return false;

            return previewType == WaveEnemyPreviewType.Tank;
        }

        private static bool IsCat(Entity enemy)
        {
            if (enemy.TryGetEnemyWavePreviewType(out WaveEnemyPreviewType previewType) == false)
                return false;

            return previewType == WaveEnemyPreviewType.Cat;
        }
    }
}
