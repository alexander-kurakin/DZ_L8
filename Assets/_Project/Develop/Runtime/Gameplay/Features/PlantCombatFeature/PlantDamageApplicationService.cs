using Assets._Project.Develop.Runtime.Configs.Gameplay.Stages;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.JuiceFeature;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.PlantCombatFeature
{
    public class PlantDamageApplicationService
    {
        private readonly PlantDamageCounterService _plantDamageCounterService;
        private readonly DragonEnrageService _dragonEnrageService;
        private readonly GameplayJuiceService _gameplayJuiceService;

        public PlantDamageApplicationService(
            PlantDamageCounterService plantDamageCounterService,
            DragonEnrageService dragonEnrageService,
            GameplayJuiceService gameplayJuiceService)
        {
            _plantDamageCounterService = plantDamageCounterService;
            _dragonEnrageService = dragonEnrageService;
            _gameplayJuiceService = gameplayJuiceService;
        }

        public bool TryApplyDamage(Entity source, Entity target, float baseDamage, PlantDamageSource damageSource)
        {
            float damageMultiplier = 1f;

            if (target.TryGetEnemyWavePreviewType(out WaveEnemyPreviewType previewType))
                damageMultiplier = _plantDamageCounterService.GetDamageMultiplier(damageSource, previewType);

            if (damageMultiplier <= 0f)
                return false;

            float damage = baseDamage * damageMultiplier;

            if (damage <= 0f)
                return false;

            bool damageApplied = EntitiesHelper.TryTakeDamageFrom(source, target, damage);

            if (damageApplied
                && damageSource == PlantDamageSource.Mine
                && previewType == WaveEnemyPreviewType.Dragon)
            {
                _dragonEnrageService.RegisterMineHit(target);

                if (target.TryGetDragonEnrageStackCount(out int stackCount))
                    _gameplayJuiceService.PlayDragonEnragePulse(target, stackCount);
            }

            return damageApplied;
        }
    }
}
