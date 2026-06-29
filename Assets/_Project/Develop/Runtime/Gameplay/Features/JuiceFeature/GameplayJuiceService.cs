using System.Collections.Generic;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Sectors;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature;
using DG.Tweening;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.JuiceFeature
{
    public class GameplayJuiceService
    {
        private readonly DragonEnrageConfig _dragonEnrageConfig;
        private readonly ScreenShakeService _screenShakeService;
        private readonly Dictionary<Transform, Vector3> _baseScaleByTransform = new();

        public GameplayJuiceService(
            EntitiesLifeContext entitiesLifeContext,
            DragonEnrageConfig dragonEnrageConfig,
            ScreenShakeService screenShakeService)
        {
            _dragonEnrageConfig = dragonEnrageConfig;
            _screenShakeService = screenShakeService;
            entitiesLifeContext.Released += OnEntityReleased;
        }

        public void PlayScreenShakeSmall()
        {
            _screenShakeService.PlaySmall();
        }

        public void PlayScreenShakeMedium()
        {
            _screenShakeService.PlayMedium();
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
            int cappedStacks = Mathf.Min(stackCount, _dragonEnrageConfig.EnragePunchMaxStacksForScale);
            float punchScale = 1f + cappedStacks * _dragonEnrageConfig.EnragePunchScalePerStack;
            float punchDurationSeconds = _dragonEnrageConfig.EnragePunchDurationSeconds;

            dragonTransform.DOKill();
            dragonTransform.localScale = baseScale;
            dragonTransform
                .DOScale(baseScale * _dragonEnrageConfig.EnragePunchScale * punchScale, punchDurationSeconds)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true)
                .OnComplete(() =>
                    dragonTransform
                        .DOScale(baseScale, punchDurationSeconds)
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
