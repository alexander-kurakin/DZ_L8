using Assets._Project.Develop.Runtime.Configs.Gameplay.Sectors;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature
{
    [RequireComponent(typeof(SectorVolumeRegistrator))]
    public class SectorView : MonoBehaviour
    {
        [SerializeField] private Renderer _fillRenderer;
        [SerializeField] private LineRenderer _outline;

        private SectorVolumeRegistrator _registrator;
        private Material _fillMaterial;
        private Tween _unlockRevealTween;
        private SectorFillVisualData _currentFillVisual;

        private void OnValidate()
        {
            _fillRenderer ??= GetComponent<Renderer>();
            _outline ??= GetComponentInChildren<LineRenderer>();
        }

        public void Apply(SectorVisualConfig visualConfig, SectorRegistryService registry)
        {
            Apply(visualConfig, registry, null, false, null);
        }

        public void Apply(
            SectorVisualConfig visualConfig,
            SectorRegistryService registry,
            IReadOnlyCollection<int> spawnPathIndices,
            bool restrictFillToSpawnPaths)
        {
            Apply(visualConfig, registry, spawnPathIndices, restrictFillToSpawnPaths, null);
        }

        public void Apply(
            SectorVisualConfig visualConfig,
            SectorRegistryService registry,
            IReadOnlyCollection<int> spawnPathIndices,
            bool restrictFillToSpawnPaths,
            IReadOnlyCollection<int> pendingUnlockRevealPathIndices)
        {
            if (visualConfig == null)
                return;

            if (_registrator == null)
                _registrator = GetComponent<SectorVolumeRegistrator>();

            if (_fillRenderer == null)
                _fillRenderer = GetComponent<Renderer>();

            bool isPathUnlocked = registry != null && registry.IsPathUnlocked(_registrator.SectorId.Index);
            bool isPendingUnlockReveal = pendingUnlockRevealPathIndices != null
                                         && pendingUnlockRevealPathIndices.Contains(_registrator.SectorId.Index);
            bool isSpawnPath = spawnPathIndices != null
                               && spawnPathIndices.Contains(_registrator.SectorId.Index);
            bool isHighlightableBelt = _registrator.SectorId.Belt != SectorBelt.Spawn;
            bool useUnlockedFill = isPathUnlocked
                                   && isPendingUnlockReveal == false
                                   && isHighlightableBelt
                                   && (restrictFillToSpawnPaths == false || isSpawnPath);

            SectorFillVisualData fillVisual = useUnlockedFill
                ? visualConfig.UnlockedFill
                : visualConfig.LockedFill;

            _currentFillVisual = fillVisual;

            if (useUnlockedFill)
            {
                if (IsUnlockRevealPlaying() == false)
                    ApplyFill(fillVisual);
            }
            else if (IsUnlockRevealPlaying() == false)
            {
                ApplyFill(fillVisual);
            }

            ApplyOutline(visualConfig);
        }

        public void PlayUnlockReveal(
            SectorFillVisualData lockedFill,
            SectorFillVisualData unlockedFill,
            float delaySeconds,
            float revealDurationSeconds)
        {
            if (_fillRenderer == null)
                return;

            _unlockRevealTween?.Kill();

            Material fillMaterial = GetFillMaterial();

            if (fillMaterial == null)
                return;

            _currentFillVisual = unlockedFill;

            SectorVisualUtility.ApplyTransparentColor(fillMaterial, lockedFill.Color, lockedFill.Alpha);

            float revealProgress = 0f;
            Sequence revealSequence = DOTween.Sequence();
            revealSequence.AppendInterval(delaySeconds);
            revealSequence.Append(DOTween
                .To(
                    () => revealProgress,
                    progress =>
                    {
                        revealProgress = progress;
                        Color color = Color.Lerp(lockedFill.Color, unlockedFill.Color, progress);
                        float alpha = Mathf.Lerp(lockedFill.Alpha, unlockedFill.Alpha, progress);
                        SectorVisualUtility.ApplyTransparentColor(fillMaterial, color, alpha);
                    },
                    1f,
                    revealDurationSeconds)
                .SetEase(Ease.OutQuad));
            revealSequence.OnKill(() => ApplyFill(unlockedFill));
            revealSequence.OnComplete(() => ApplyFill(unlockedFill));
            revealSequence.SetUpdate(true);

            _unlockRevealTween = revealSequence.Play();
        }

        private void OnDestroy()
        {
            StopUnlockRevealIfPlaying();
        }

        private void StopUnlockRevealIfPlaying()
        {
            if (_unlockRevealTween == null)
                return;

            _unlockRevealTween.Kill();
            _unlockRevealTween = null;
        }

        private bool IsUnlockRevealPlaying()
        {
            return _unlockRevealTween != null && _unlockRevealTween.IsActive();
        }

        private void ApplyFill(SectorFillVisualData fillVisual)
        {
            Material fillMaterial = GetFillMaterial();

            if (fillMaterial == null)
                return;

            SectorVisualUtility.ApplyTransparentColor(fillMaterial, fillVisual.Color, fillVisual.Alpha);
        }

        private Material GetFillMaterial()
        {
            if (_fillRenderer == null)
                return null;

            if (_fillMaterial == null)
                _fillMaterial = _fillRenderer.material;

            return _fillMaterial;
        }

        private void ApplyOutline(SectorVisualConfig visualConfig)
        {
            if (_outline == null)
                _outline = GetComponentInChildren<LineRenderer>();

            if (_outline == null)
                return;

            SectorVisualUtility.ConfigureOutline(_outline, visualConfig);
        }
    }
}
