using Assets._Project.Develop.Runtime.Configs.Gameplay.Spellcore;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Stages;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.PlantBuildingBuff;
using Assets._Project.Develop.Runtime.Gameplay.Features.JuiceFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.TakeDamage;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.PlantCombatFeature
{
    public class PlantDamageApplicationService
    {
        private readonly PlantDamageCounterService _plantDamageCounterService;
        private readonly TankMineShieldService _tankMineShieldService;
        private readonly SpellcoreCombatConfig _spellcoreCombatConfig;
        private readonly PlantBuildingBuffService _plantBuildingBuffService;

        public PlantDamageApplicationService(
            PlantDamageCounterService plantDamageCounterService,
            TankMineShieldService tankMineShieldService,
            SpellcoreCombatConfig spellcoreCombatConfig,
            PlantBuildingBuffService plantBuildingBuffService)
        {
            _plantDamageCounterService = plantDamageCounterService;
            _tankMineShieldService = tankMineShieldService;
            _spellcoreCombatConfig = spellcoreCombatConfig;
            _plantBuildingBuffService = plantBuildingBuffService;
        }

        public bool TryApplyDamage(Entity source, Entity target, float baseDamage, PlantDamageSource damageSource)
        {
            float damageMultiplier = 1f;
            bool hasPreviewType = target.TryGetEnemyWavePreviewType(out WaveEnemyPreviewType previewType);

            if (hasPreviewType)
            {
                damageMultiplier = _plantDamageCounterService.GetDamageMultiplier(damageSource, previewType);

                if (damageSource == PlantDamageSource.Mine
                    && previewType == WaveEnemyPreviewType.Tank)
                {
                    float tankShieldMultiplier = _tankMineShieldService.ResolveMineDamageMultiplier(
                        target,
                        previewType,
                        _spellcoreCombatConfig.TankFirstMinePulseDamageMultiplier);

                    damageMultiplier *= tankShieldMultiplier;
                }
            }

            if (damageMultiplier <= 0f)
                return false;

            float damage = baseDamage * damageMultiplier;
            damage *= _plantBuildingBuffService.GetDamageMultiplier(source);

            if (damage <= 0f)
                return false;

            TakeDamageVisualKind visualKind = damageSource switch
            {
                PlantDamageSource.Mine => TakeDamageVisualKind.Mine,
                PlantDamageSource.Toxic => TakeDamageVisualKind.Toxic,
                PlantDamageSource.Turret => TakeDamageVisualKind.Turret,
                _ => TakeDamageVisualKind.Default,
            };

            return EntitiesHelper.TryTakeDamageFrom(source, target, damage, visualKind);
        }
    }
}
