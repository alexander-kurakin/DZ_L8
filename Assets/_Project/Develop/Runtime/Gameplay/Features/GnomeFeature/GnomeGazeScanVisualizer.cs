using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.GnomeFeature
{
    public class GnomeGazeScanVisualizer : MonoBehaviour, IGnomeGazeScanVisualizer
    {
        private const int CIRCLE_SEGMENTS = 36;
        private const float LINE_WIDTH = 0.03f;

        private LineRenderer _lineRenderer;

        private void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();

            if (_lineRenderer == null)
                _lineRenderer = gameObject.AddComponent<LineRenderer>();

            _lineRenderer.loop = true;
            _lineRenderer.useWorldSpace = true;
            _lineRenderer.positionCount = CIRCLE_SEGMENTS;
            _lineRenderer.startWidth = LINE_WIDTH;
            _lineRenderer.endWidth = LINE_WIDTH;
            _lineRenderer.numCapVertices = 4;
            _lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _lineRenderer.receiveShadows = false;
            _lineRenderer.enabled = false;

            if (_lineRenderer.sharedMaterial == null)
            {
                Shader shader = Shader.Find("Sprites/Default");

                if (shader != null)
                    _lineRenderer.sharedMaterial = new Material(shader);
            }

            _lineRenderer.startColor = new Color(1f, 0.92f, 0.016f, 0.85f);
            _lineRenderer.endColor = new Color(1f, 0.92f, 0.016f, 0.85f);
        }

        public void SetScanContour(Vector3 center, Vector3 surfaceNormal, float radius, bool isVisible)
        {
            if (_lineRenderer == null)
                return;

            _lineRenderer.enabled = isVisible;

            if (isVisible == false)
                return;

            Vector3 normal = surfaceNormal.normalized;
            Vector3 tangent = Vector3.Cross(normal, Vector3.up);

            if (tangent.sqrMagnitude <= 0.0001f)
                tangent = Vector3.Cross(normal, Vector3.right);

            tangent.Normalize();
            Vector3 bitangent = Vector3.Cross(normal, tangent).normalized;

            for (int segmentIndex = 0; segmentIndex < CIRCLE_SEGMENTS; segmentIndex++)
            {
                float angle = segmentIndex / (float)CIRCLE_SEGMENTS * Mathf.PI * 2f;
                Vector3 offset = tangent * Mathf.Cos(angle) + bitangent * Mathf.Sin(angle);
                _lineRenderer.SetPosition(segmentIndex, center + offset * radius);
            }
        }
    }
}
