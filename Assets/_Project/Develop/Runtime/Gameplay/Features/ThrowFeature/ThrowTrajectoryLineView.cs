using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System.Collections.Generic;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.ThrowFeature
{
    public class ThrowTrajectoryLineView : EntityView
    {
        private const float LINE_START_WIDTH = 0.12f;
        private const float LINE_END_WIDTH = 0.04f;
        private const float DASH_WORLD_SIZE = 0.35f;
        private const int DOTTED_TEXTURE_WIDTH = 16;
        private const int DOTTED_TEXTURE_DASH_PIXELS = 8;

        private static readonly Color LineColor = new Color(1f, 1f, 1f, 0.9f);

        private IReadOnlyVariable<bool> _isVisible;
        private List<ReactiveVariable<Vector3>> _trajectoryPoints;
        private LineRenderer _lineRenderer;
        private Material _lineMaterial;
        private Texture2D _dottedTexture;

        protected override void OnEntityStartedWork(Entity entity)
        {
            ThrowTrajectoryPreview trajectoryPreview = entity.GetComponent<ThrowTrajectoryPreview>();
            _isVisible = trajectoryPreview.IsVisible;
            _trajectoryPoints = trajectoryPreview.TrajectoryPoints;

            SetupLineRenderer();
        }

        public override void Cleanup(Entity entity)
        {
            base.Cleanup(entity);

            if (_lineRenderer != null)
                _lineRenderer.positionCount = 0;
        }

        private void OnDestroy()
        {
            if (_lineMaterial != null)
                Destroy(_lineMaterial);

            if (_dottedTexture != null)
                Destroy(_dottedTexture);
        }

        private void LateUpdate()
        {
            if (_lineRenderer == null)
                return;

            if (_isVisible == null || _isVisible.Value == false || _trajectoryPoints == null || _trajectoryPoints.Count < 2)
            {
                _lineRenderer.positionCount = 0;
                return;
            }

            _lineRenderer.positionCount = _trajectoryPoints.Count;

            for (int pointIndex = 0; pointIndex < _trajectoryPoints.Count; pointIndex++)
                _lineRenderer.SetPosition(pointIndex, _trajectoryPoints[pointIndex].Value);

            UpdateDottedTextureScale();
        }

        private void SetupLineRenderer()
        {
            _lineRenderer = gameObject.GetComponent<LineRenderer>();

            if (_lineRenderer == null)
                _lineRenderer = gameObject.AddComponent<LineRenderer>();

            _dottedTexture = CreateDottedTexture();
            _lineMaterial = new Material(Shader.Find("Sprites/Default"));
            _lineMaterial.mainTexture = _dottedTexture;

            _lineRenderer.useWorldSpace = true;
            _lineRenderer.loop = false;
            _lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _lineRenderer.receiveShadows = false;
            _lineRenderer.textureMode = LineTextureMode.Tile;
            _lineRenderer.alignment = LineAlignment.View;
            _lineRenderer.startWidth = LINE_START_WIDTH;
            _lineRenderer.endWidth = LINE_END_WIDTH;
            _lineRenderer.startColor = LineColor;
            _lineRenderer.endColor = LineColor;
            _lineRenderer.positionCount = 0;
            _lineRenderer.material = _lineMaterial;
        }

        private void UpdateDottedTextureScale()
        {
            float totalLength = 0f;

            for (int pointIndex = 1; pointIndex < _trajectoryPoints.Count; pointIndex++)
            {
                Vector3 previousPoint = _trajectoryPoints[pointIndex - 1].Value;
                Vector3 currentPoint = _trajectoryPoints[pointIndex].Value;
                totalLength += Vector3.Distance(previousPoint, currentPoint);
            }

            if (totalLength <= 0f)
                return;

            _lineMaterial.mainTextureScale = new Vector2(totalLength / DASH_WORLD_SIZE, 1f);
        }

        private static Texture2D CreateDottedTexture()
        {
            Texture2D texture = new Texture2D(DOTTED_TEXTURE_WIDTH, 1, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Repeat;
            texture.filterMode = FilterMode.Point;

            for (int pixelIndex = 0; pixelIndex < DOTTED_TEXTURE_WIDTH; pixelIndex++)
            {
                bool isDashPixel = pixelIndex < DOTTED_TEXTURE_DASH_PIXELS;
                Color pixelColor = isDashPixel ? Color.white : Color.clear;
                texture.SetPixel(pixelIndex, 0, pixelColor);
            }

            texture.Apply();
            return texture;
        }
    }
}
