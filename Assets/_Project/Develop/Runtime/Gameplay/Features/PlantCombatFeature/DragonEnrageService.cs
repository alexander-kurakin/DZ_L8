using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Stages;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.PlantCombatFeature
{
    public class DragonEnrageService
    {
        private readonly DragonEnrageConfig _config;

        public DragonEnrageService(DragonEnrageConfig config)
        {
            _config = config;
        }

        public float EnrageEffectScalePerStack => _config.EnrageEffectScalePerStack;

        public void RegisterMineHit(Entity dragon)
        {
            if (dragon.TryGetEnemyWavePreviewType(out WaveEnemyPreviewType previewType) == false)
                return;

            if (previewType != WaveEnemyPreviewType.Dragon)
                return;

            if (dragon.TryGetDragonEnrageStackCount(out int stackCount) == false)
                return;

            if (stackCount >= _config.MaxEnrageStacks)
                return;

            dragon.DragonEnrageStackCountC.Value = stackCount + 1;
        }

        public float GetOutgoingDamageMultiplier(Entity dragon)
        {
            if (dragon.TryGetDragonEnrageStackCount(out int stackCount) == false)
                return 1f;

            return 1f + stackCount * _config.OutgoingDamageBonusPerStack;
        }
    }
}
