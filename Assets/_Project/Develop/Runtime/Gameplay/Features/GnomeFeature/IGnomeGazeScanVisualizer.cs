using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.GnomeFeature
{
    public interface IGnomeGazeScanVisualizer
    {
        void SetScanContour(Vector3 center, Vector3 surfaceNormal, float radius, bool isVisible);
    }
}
