using Assets._Project.Develop.Runtime.Configs.Gameplay.Stages;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.PlantCombatFeature
{
    public class DragonEnrageService
    {
        private const float OUTGOING_DAMAGE_BONUS_PER_STACK = 0.5f;

        public void RegisterMineHit(Entity dragon)
        {
            if (dragon.TryGetEnemyWavePreviewType(out WaveEnemyPreviewType previewType) == false)
                return;

            if (previewType != WaveEnemyPreviewType.Dragon)
                return;

            if (dragon.TryGetDragonEnrageStackCount(out int stackCount) == false)
                return;

            dragon.DragonEnrageStackCountC.Value = stackCount + 1;
        }

        public float GetOutgoingDamageMultiplier(Entity dragon)
        {
            if (dragon.TryGetDragonEnrageStackCount(out int stackCount) == false)
                return 1f;

            return 1f + stackCount * OUTGOING_DAMAGE_BONUS_PER_STACK;
        }
    }
}
