using Assets._Project.Develop.Runtime.Configs.Gameplay.Stages;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature;
using Assets._Project.Develop.Runtime.Utilities.Reactive;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.PlantCombatFeature
{
    public static class TurretTargetPriority
    {
        public static bool IsDragon(Entity entity)
        {
            if (entity.HasComponent<FlyingEnemy>())
                return true;

            if (entity.TryGetEnemyWavePreviewType(out WaveEnemyPreviewType previewType) == false)
                return false;

            return previewType == WaveEnemyPreviewType.Dragon;
        }

        public static bool IsInExactSector(Entity enemy, SectorId plantSector)
        {
            if (enemy.TryGetCurrentSector(out ReactiveVariable<SectorId> currentSector) == false)
                return false;

            return currentSector.Value.Equals(plantSector);
        }
    }
}
