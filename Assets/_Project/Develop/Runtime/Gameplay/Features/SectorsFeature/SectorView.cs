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

        public void Apply(SectorVisualConfig visualConfig, SectorRegistryService registry)
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
            ApplyOutline(visualConfig);
        }

        private void ApplyFill(SectorFillVisualData fillVisual)
        {
            if (_fillRenderer == null)
                return;

            Material material = _fillRenderer.material;
            SectorVisualUtility.ApplyTransparentColor(material, fillVisual.Color, fillVisual.Alpha);
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
