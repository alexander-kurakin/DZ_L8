using Assets._Project.Develop.Runtime.Configs.Gameplay.Stages;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.JuiceFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.TakeDamage;
using UnityEngine;

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

            TakeDamageVisualKind visualKind = damageSource == PlantDamageSource.Mine
                ? TakeDamageVisualKind.Mine
                : TakeDamageVisualKind.Default;

            bool damageApplied = EntitiesHelper.TryTakeDamageFrom(source, target, damage, visualKind);

            if (damageApplied
                && damageSource == PlantDamageSource.Mine
                && previewType == WaveEnemyPreviewType.Dragon)
            {
                _dragonEnrageService.RegisterMineHit(target);

                if (target.TryGetDragonEnrageStackCount(out int stackCount))
                {
                    _gameplayJuiceService.PlayDragonEnragePulse(target, stackCount);

                    if (target.TryGetTransform(out Transform dragonTransform))
                    {
                        DragonEnrageView enrageView = dragonTransform.GetComponentInChildren<DragonEnrageView>(true);
                        enrageView?.PlayEnrage(stackCount, _dragonEnrageService.EnrageEffectScalePerStack);
                    }
                }
            }

            return damageApplied;
        }
    }
}
