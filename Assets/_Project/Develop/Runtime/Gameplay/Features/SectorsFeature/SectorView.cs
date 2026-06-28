using Assets._Project.Develop.Runtime.Configs.Gameplay.Sectors;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature
{
    [RequireComponent(typeof(SectorVolumeRegistrator))]
    public class SectorView : MonoBehaviour
    {
        [SerializeField] private Renderer _fillRenderer;
        [SerializeField] private LineRenderer _outline;

        private SectorVolumeRegistrator _registrator;

        private void OnValidate()
        {
            _fillRenderer ??= GetComponent<Renderer>();
            _outline ??= GetComponentInChildren<LineRenderer>();
        }

        public void Apply(SectorVisualConfig visualConfig, SectorRegistryService registry, bool showSpawnPathPreview)
        {
            if (visualConfig == null)
                return;

            if (_registrator == null)
                _registrator = GetComponent<SectorVolumeRegistrator>();

            if (_fillRenderer == null)
                _fillRenderer = GetComponent<Renderer>();

            bool isPathUnlocked = registry != null && registry.IsPathUnlocked(_registrator.SectorId.Index);
            SectorFillVisualData fillVisual = isPathUnlocked
                ? visualConfig.UnlockedFill
                : visualConfig.LockedFill;

            ApplyFill(fillVisual);

            bool highlightSpawnPath = showSpawnPathPreview
                                      && _registrator.SectorId.Belt == SectorBelt.Spawn
                                      && isPathUnlocked;

            ApplyOutline(visualConfig, highlightSpawnPath);
        }

        private void ApplyFill(SectorFillVisualData fillVisual)
        {
            if (_fillRenderer == null)
                return;

            Material material = _fillRenderer.material;
            SectorVisualUtility.ApplyTransparentColor(material, fillVisual.Color, fillVisual.Alpha);
        }

        private void ApplyOutline(SectorVisualConfig visualConfig, bool highlightSpawnPath)
        {
            if (_outline == null)
                _outline = GetComponentInChildren<LineRenderer>();

            if (_outline == null)
                return;

            if (highlightSpawnPath)
                SectorVisualUtility.ConfigureOutline(
                    _outline,
                    visualConfig.SpawnPreviewOutlineColor,
                    visualConfig.SpawnPreviewOutlineWidth);
            else
                SectorVisualUtility.ConfigureOutline(_outline, visualConfig);
        }
    }
}
