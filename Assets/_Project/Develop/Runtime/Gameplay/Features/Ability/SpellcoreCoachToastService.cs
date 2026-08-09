using System.Collections.Generic;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Stages;
using Assets._Project.Develop.Runtime.Gameplay.Features.PlantPlacementFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Ability
{
    public class SpellcoreCoachToastService
    {
        private const float INVALID_PLACE_HINT_COOLDOWN_SECONDS = 3f;

        private readonly LmbFlavorToastService _lmbFlavorToastService;
        private readonly PlantPlacementService _plantPlacementService;
        private readonly SectorMembershipService _sectorMembershipService;

        public SpellcoreCoachToastService(
            LmbFlavorToastService lmbFlavorToastService,
            PlantPlacementService plantPlacementService,
            SectorMembershipService sectorMembershipService)
        {
            _lmbFlavorToastService = lmbFlavorToastService;
            _plantPlacementService = plantPlacementService;
            _sectorMembershipService = sectorMembershipService;
        }

        public void InitializeForRun()
        {
            _lmbFlavorToastService.ResetForRun();
        }

        public void TryShowPreparationHints(
            int upcomingWaveNumber,
            IReadOnlyList<WaveEnemyPreviewType> upcomingWavePreviewTypes)
        {
            if (upcomingWaveNumber == 1)
                _lmbFlavorToastService.ShowOnce(LmbFlavorToastType.FreeMineHint);

            if (ContainsPreviewType(upcomingWavePreviewTypes, WaveEnemyPreviewType.Tank))
                _lmbFlavorToastService.ShowOnce(LmbFlavorToastType.WaveTwoTanksHint);

            if (ContainsPreviewType(upcomingWavePreviewTypes, WaveEnemyPreviewType.Dragon))
                _lmbFlavorToastService.ShowOnce(LmbFlavorToastType.DragonPoisonImmune);
        }

        public void TryShowFreeMineSoldHint(bool isMine, int refund)
        {
            if (isMine == false)
                return;

            if (refund > 0)
                return;

            _lmbFlavorToastService.ShowOnce(LmbFlavorToastType.FreeMineSold);
        }

        public void TryShowFirstMinePlacedGoHint()
        {
            _lmbFlavorToastService.ShowOnce(LmbFlavorToastType.FirstMinePlacedGoHint);
        }

        public void TryShowInvalidPlaceHint(Vector3 clickWorldPosition, AbilityType abilityType)
        {
            if (PlantPlacementService.IsPlantAbility(abilityType) == false)
                return;

            SectorId sectorId = _sectorMembershipService.ResolveSectorAtClick(clickWorldPosition);
            PlantPlacementPreviewState previewState =
                _plantPlacementService.GetSectorPreviewState(sectorId, abilityType);

            if (previewState != PlantPlacementPreviewState.BlockedInPrinciple)
                return;

            _lmbFlavorToastService.ShowThrottled(
                LmbFlavorToastType.InvalidPlaceHint,
                INVALID_PLACE_HINT_COOLDOWN_SECONDS);
        }

        private static bool ContainsPreviewType(
            IReadOnlyList<WaveEnemyPreviewType> previewTypes,
            WaveEnemyPreviewType targetType)
        {
            if (previewTypes == null)
                return false;

            for (int index = 0; index < previewTypes.Count; index++)
            {
                if (previewTypes[index] == targetType)
                    return true;
            }

            return false;
        }
    }
}
