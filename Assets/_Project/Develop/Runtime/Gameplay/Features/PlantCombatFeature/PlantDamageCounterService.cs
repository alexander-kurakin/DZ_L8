using Assets._Project.Develop.Runtime.Configs.Gameplay.Stages;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.PlantCombatFeature
{
    public class PlantDamageCounterService
    {
        public float GetDamageMultiplier(PlantDamageSource source, WaveEnemyPreviewType enemyType)
        {
            switch (source)
            {
                case PlantDamageSource.Mine:
                    return 1f;

                case PlantDamageSource.Toxic:
                    switch (enemyType)
                    {
                        case WaveEnemyPreviewType.Cat:
                            return 1f;

                        case WaveEnemyPreviewType.Tank:
                            return 0.5f;

                        case WaveEnemyPreviewType.Dragon:
                            return 0f;
                    }

                    break;

                case PlantDamageSource.Turret:
                    switch (enemyType)
                    {
                        case WaveEnemyPreviewType.Cat:
                            return 1f;

                        case WaveEnemyPreviewType.Tank:
                            return 0.5f;

                        case WaveEnemyPreviewType.Dragon:
                            return 1f;
                    }

                    break;
            }

            return 1f;
        }

        public bool ShouldApplyToxicSlow(WaveEnemyPreviewType enemyType)
        {
            return enemyType != WaveEnemyPreviewType.Dragon;
        }
    }
}
