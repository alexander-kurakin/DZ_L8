using System.Collections.Generic;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Sectors;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature;
using DG.Tweening;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.JuiceFeature
{
    public class GameplayJuiceService
    {
        private const float DRAGON_ENRAGE_PUNCH_SCALE = 1.15f;
        private const float DRAGON_ENRAGE_PUNCH_DURATION_SECONDS = 0.2f;

        private readonly Dictionary<Transform, Vector3> _baseScaleByTransform = new();

        public GameplayJuiceService(EntitiesLifeContext entitiesLifeContext)
        {
            entitiesLifeContext.Released += OnEntityReleased;
        }

        public void PlayPathUnlockPulse(
            SectorBootstrap sectorBootstrap,
            SectorVisualConfig visualConfig,
            SectorRegistryService registry,
            IReadOnlyList<int> unlockedPathIndices)
        {
            if (sectorBootstrap == null || visualConfig == null || registry == null)
                return;

            if (unlockedPathIndices == null || unlockedPathIndices.Count == 0)
                return;

            sectorBootstrap.AnimatePathUnlockReveal(unlockedPathIndices, visualConfig, registry);
        }

        public void PlayDragonEnragePulse(Entity dragon, int stackCount)
        {
            if (dragon.TryGetTransform(out Transform dragonTransform) == false)
                return;

            if (_baseScaleByTransform.ContainsKey(dragonTransform) == false)
                _baseScaleByTransform[dragonTransform] = dragonTransform.localScale;

            Vector3 baseScale = _baseScaleByTransform[dragonTransform];
            float punchScale = 1f + Mathf.Min(stackCount, 4) * 0.04f;

            dragonTransform.DOKill();
            dragonTransform.localScale = baseScale;
            dragonTransform
                .DOScale(baseScale * DRAGON_ENRAGE_PUNCH_SCALE * punchScale, DRAGON_ENRAGE_PUNCH_DURATION_SECONDS)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true)
                .OnComplete(() =>
                    dragonTransform
                        .DOScale(baseScale, DRAGON_ENRAGE_PUNCH_DURATION_SECONDS)
                        .SetEase(Ease.InQuad)
                        .SetUpdate(true)
                        .Play())
                .Play();
        }

        private void OnEntityReleased(Entity entity)
        {
            if (entity.TryGetTransform(out Transform entityTransform) == false)
                return;

            _baseScaleByTransform.Remove(entityTransform);
        }
    }
}
