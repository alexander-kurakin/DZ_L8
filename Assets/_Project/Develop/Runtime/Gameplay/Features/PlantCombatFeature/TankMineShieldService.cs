using System.Collections.Generic;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Stages;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.PlantCombatFeature
{
    public class TankMineShieldService
    {
        private readonly HashSet<Entity> _tanksWithFirstMinePulseConsumed = new();

        public float ResolveMineDamageMultiplier(Entity target, WaveEnemyPreviewType previewType, float firstPulseMultiplier)
        {
            if (previewType != WaveEnemyPreviewType.Tank)
                return 1f;

            if (_tanksWithFirstMinePulseConsumed.Contains(target))
                return 1f;

            _tanksWithFirstMinePulseConsumed.Add(target);
            return firstPulseMultiplier;
        }
    }
}
