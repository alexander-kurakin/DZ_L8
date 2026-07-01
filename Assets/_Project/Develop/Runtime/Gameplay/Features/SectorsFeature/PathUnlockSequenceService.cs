using System;
using System.Collections.Generic;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Sectors;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Stages;
using Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature;
using DG.Tweening;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature
{
    public class PathUnlockSequenceService
    {
        private readonly SpawnPathPreviewService _spawnPathPreviewService;

        private Tween _activeSequenceTween;
        private IReadOnlyList<int> _revealingPathIndices;
        private readonly HashSet<SectorBelt> _revealedPlantMarkerBelts = new();

        public PathUnlockSequenceService(SpawnPathPreviewService spawnPathPreviewService)
        {
            _spawnPathPreviewService = spawnPathPreviewService;
        }

        public event Action<IReadOnlyList<int>> UnlockSequenceStarted;

        public event Action<SectorBelt, IReadOnlyList<int>> RevealPlantMarkersForBelt;

        public event Action UnlockSequenceCompleted;

        public bool IsPlaying { get; private set; }

        public IReadOnlyList<int> RevealingPathIndices => _revealingPathIndices;

        public bool IsPlantMarkerBeltRevealed(SectorBelt belt)
        {
            return _revealedPlantMarkerBelts.Contains(belt);
        }

        public void Play(
            IReadOnlyList<int> pathIndices,
            SectorBootstrap sectorBootstrap,
            SectorVisualConfig visualConfig,
            SectorRegistryService registry,
            WaveEnemyPreviewIconsConfig enemyIconsConfig,
            IReadOnlyList<SpawnGroupPlanEntry> groupPlans)
        {
            Cancel();

            if (pathIndices == null || pathIndices.Count == 0)
                return;

            if (sectorBootstrap == null || visualConfig == null || registry == null)
                return;

            _revealingPathIndices = pathIndices;
            _revealedPlantMarkerBelts.Clear();
            IsPlaying = true;
            UnlockSequenceStarted?.Invoke(pathIndices);

            sectorBootstrap.AnimatePathUnlockReveal(pathIndices, visualConfig, registry);

            float beltStepSeconds = visualConfig.BeltRevealStepSeconds;
            Sequence revealSequence = DOTween.Sequence();

            revealSequence.AppendInterval(beltStepSeconds);
            revealSequence.AppendCallback(() => RevealPlantMarkers(SectorBelt.Inner, pathIndices));

            revealSequence.AppendInterval(beltStepSeconds);
            revealSequence.AppendCallback(() => RevealPlantMarkers(SectorBelt.Middle, pathIndices));

            revealSequence.AppendInterval(beltStepSeconds);
            revealSequence.AppendCallback(() =>
            {
                RevealPlantMarkers(SectorBelt.Outer, pathIndices);
                ShowSpawnPathPreview(registry, visualConfig, enemyIconsConfig, groupPlans);
            });

            revealSequence.OnComplete(OnSequenceCompleted);

            _activeSequenceTween = revealSequence.Play();
        }

        public void Cancel()
        {
            _activeSequenceTween?.Kill();
            _activeSequenceTween = null;
            IsPlaying = false;
            _revealingPathIndices = null;
            _revealedPlantMarkerBelts.Clear();
        }

        private void RevealPlantMarkers(SectorBelt belt, IReadOnlyList<int> pathIndices)
        {
            _revealedPlantMarkerBelts.Add(belt);
            RevealPlantMarkersForBelt?.Invoke(belt, pathIndices);
        }

        private void OnSequenceCompleted()
        {
            _activeSequenceTween = null;
            IsPlaying = false;
            _revealingPathIndices = null;
            _revealedPlantMarkerBelts.Clear();
            UnlockSequenceCompleted?.Invoke();
        }

        private void ShowSpawnPathPreview(
            SectorRegistryService registry,
            SectorVisualConfig visualConfig,
            WaveEnemyPreviewIconsConfig enemyIconsConfig,
            IReadOnlyList<SpawnGroupPlanEntry> groupPlans)
        {
            _spawnPathPreviewService.Refresh(
                true,
                registry,
                visualConfig,
                enemyIconsConfig,
                groupPlans);
        }
    }
}
