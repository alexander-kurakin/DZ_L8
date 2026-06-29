using System;
using System.Collections.Generic;
using Assets._Project.Develop.Runtime.Gameplay.Features.Ability;
using Assets._Project.Develop.Runtime.Gameplay.Features.PlantPlacementFeature;
using DG.Tweening;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature
{
    public class PlantPlacementPreviewService
    {
        private const float MARKER_GROUND_Y_OFFSET = 0.55f;
        private const float WEDGE_ANGLE_FRACTION = 0.18f;
        private const float BELT_RADIUS_FRACTION = 0.72f;
        private const float INNER_WEDGE_ANGLE_FRACTION = 0.5f;
        private const float INNER_BELT_RADIUS_FRACTION = 0.84f;
        private const float MARKER_WORLD_SIZE = 7f;
        private const float CHECK_SPRITE_Z_ROTATION = -90f;
        private const int MARKER_SORTING_ORDER = 175;
        private const float HOVER_CROSS_APPEAR_DURATION_SECONDS = 0.18f;
        private const float HOVER_CROSS_SCALE_FROM = 0.55f;

        private static readonly Color CHECK_COLOR = new Color(0.25f, 0.95f, 0.35f, 1f);
        private static readonly Color CROSS_COLOR = new Color(0.95f, 0.25f, 0.25f, 1f);

        private static readonly SectorBelt[] PLANTABLE_BELTS =
        {
            SectorBelt.Inner,
            SectorBelt.Middle,
            SectorBelt.Outer,
        };

        private static Sprite _checkSprite;
        private static Sprite _crossSprite;

        private readonly Dictionary<SectorId, GameObject> _markerBySectorId = new();
        private Transform _root;
        private GameObject _hoverCrossMarker;
        private Transform _hoverCrossVisual;
        private SpriteRenderer _hoverCrossRenderer;
        private float _hoverCrossBaseScale = 1f;
        private SectorId? _hoveredSectorId;
        private Tween _hoverCrossTween;

        public void Refresh(
            bool visible,
            SectorRegistryService registry,
            PlantPlacementService plantPlacementService,
            AbilityType abilityType)
        {
            ClearMarkers();

            if (visible == false)
                return;

            if (CanBuildMarkers(registry, plantPlacementService, abilityType) == false)
                return;

            IReadOnlyList<int> unlockedPathIndices = registry.UnlockedPathIndices;

            for (int pathListIndex = 0; pathListIndex < unlockedPathIndices.Count; pathListIndex++)
            {
                int pathIndex = unlockedPathIndices[pathListIndex];
                EnsureMarkersForPath(registry, plantPlacementService, abilityType, pathIndex, null);
            }
        }

        public void EnsureMarkersForPath(
            SectorRegistryService registry,
            PlantPlacementService plantPlacementService,
            AbilityType abilityType,
            int pathIndex,
            SectorBelt? beltFilter)
        {
            if (CanBuildMarkers(registry, plantPlacementService, abilityType) == false)
                return;

            if (registry.IsPathUnlocked(pathIndex) == false)
                return;

            for (int beltIndex = 0; beltIndex < PLANTABLE_BELTS.Length; beltIndex++)
            {
                SectorBelt belt = PLANTABLE_BELTS[beltIndex];

                if (beltFilter.HasValue && beltFilter.Value != belt)
                    continue;

                SectorId sectorId = new SectorId(belt, pathIndex);
                UpsertMarker(registry, plantPlacementService, abilityType, sectorId);
            }
        }

        public void UpdateHoverCross(
            SectorId hoveredSectorId,
            SectorRegistryService registry,
            PlantPlacementService plantPlacementService,
            AbilityType abilityType)
        {
            if (CanBuildMarkers(registry, plantPlacementService, abilityType) == false)
            {
                ClearHoverCross();
                return;
            }

            PlantPlacementPreviewState previewState =
                plantPlacementService.GetSectorPreviewState(hoveredSectorId, abilityType);

            if (previewState != PlantPlacementPreviewState.BlockedOccupied)
            {
                ClearHoverCross();
                return;
            }

            if (_hoveredSectorId.HasValue && _hoveredSectorId.Value == hoveredSectorId)
                return;

            _hoveredSectorId = hoveredSectorId;
            Vector3 markerPosition = GetMarkerPosition(registry, hoveredSectorId);
            ShowHoverCross(markerPosition);
        }

        public void ClearHoverCross()
        {
            _hoverCrossTween?.Kill();
            _hoverCrossTween = null;
            _hoveredSectorId = null;

            if (_hoverCrossMarker != null)
                _hoverCrossMarker.SetActive(false);
        }

        public void ClearMarkers()
        {
            ClearHoverCross();

            foreach (KeyValuePair<SectorId, GameObject> markerEntry in _markerBySectorId)
            {
                if (markerEntry.Value != null)
                    Object.Destroy(markerEntry.Value);
            }

            _markerBySectorId.Clear();
        }

        private bool CanBuildMarkers(
            SectorRegistryService registry,
            PlantPlacementService plantPlacementService,
            AbilityType abilityType)
        {
            if (registry == null || registry.IsInitialized == false)
                return false;

            if (plantPlacementService == null)
                return false;

            if (PlantPlacementService.IsPlantAbility(abilityType) == false)
                return false;

            EnsureRoot();
            return true;
        }

        private void UpsertMarker(
            SectorRegistryService registry,
            PlantPlacementService plantPlacementService,
            AbilityType abilityType,
            SectorId sectorId)
        {
            if (_markerBySectorId.TryGetValue(sectorId, out GameObject existingMarker))
            {
                if (existingMarker != null)
                    Object.Destroy(existingMarker);

                _markerBySectorId.Remove(sectorId);
            }

            PlantPlacementPreviewState previewState =
                plantPlacementService.GetSectorPreviewState(sectorId, abilityType);

            if (previewState == PlantPlacementPreviewState.BlockedOccupied)
                return;

            Vector3 markerPosition = GetMarkerPosition(registry, sectorId);
            bool showCheck = previewState == PlantPlacementPreviewState.Allowed;
            GameObject marker = CreatePlacementMarker(markerPosition, showCheck);
            marker.transform.SetParent(_root, false);
            _markerBySectorId[sectorId] = marker;
        }

        private static Vector3 GetMarkerPosition(SectorRegistryService registry, SectorId sectorId)
        {
            return WorldToSector.GetSectorWedgeMarkerPosition(
                registry.Center,
                sectorId,
                registry.GridConfig,
                GetWedgeAngleFraction(sectorId.Belt),
                GetBeltRadiusFraction(sectorId.Belt),
                MARKER_GROUND_Y_OFFSET);
        }

        private void ShowHoverCross(Vector3 markerPosition)
        {
            EnsureHoverCrossMarker();
            _hoverCrossMarker.transform.SetPositionAndRotation(markerPosition, Quaternion.Euler(90f, 0f, 0f));
            _hoverCrossMarker.SetActive(true);

            _hoverCrossTween?.Kill();
            _hoverCrossVisual.localScale = Vector3.one * (_hoverCrossBaseScale * HOVER_CROSS_SCALE_FROM);

            Color hiddenColor = CROSS_COLOR;
            hiddenColor.a = 0f;
            _hoverCrossRenderer.color = hiddenColor;

            _hoverCrossTween = DOTween.Sequence()
                .Append(_hoverCrossVisual
                    .DOScale(Vector3.one * _hoverCrossBaseScale, HOVER_CROSS_APPEAR_DURATION_SECONDS)
                    .SetEase(Ease.OutBack))
                .Join(_hoverCrossRenderer
                    .DOColor(CROSS_COLOR, HOVER_CROSS_APPEAR_DURATION_SECONDS)
                    .SetEase(Ease.OutQuad))
                .SetUpdate(true)
                .Play();
        }

        private void EnsureHoverCrossMarker()
        {
            if (_hoverCrossMarker != null)
                return;

            _hoverCrossMarker = new GameObject("PlantPlacementHoverCross");
            _hoverCrossMarker.transform.SetParent(_root, false);
            _hoverCrossMarker.transform.SetPositionAndRotation(Vector3.zero, Quaternion.Euler(90f, 0f, 0f));

            Sprite crossSprite = GetOrCreateCrossSprite();
            _hoverCrossBaseScale = GetScaleForWorldSize(crossSprite, MARKER_WORLD_SIZE);
            _hoverCrossVisual = CreateWorldSpriteChild(
                _hoverCrossMarker.transform,
                "Marker",
                crossSprite,
                Vector3.zero,
                MARKER_WORLD_SIZE,
                CROSS_COLOR,
                MARKER_SORTING_ORDER).transform;
            _hoverCrossRenderer = _hoverCrossVisual.GetComponent<SpriteRenderer>();
        }

        private static float GetBeltRadiusFraction(SectorBelt belt)
        {
            if (belt == SectorBelt.Inner)
                return INNER_BELT_RADIUS_FRACTION;

            return BELT_RADIUS_FRACTION;
        }

        private static float GetWedgeAngleFraction(SectorBelt belt)
        {
            if (belt == SectorBelt.Inner)
                return INNER_WEDGE_ANGLE_FRACTION;

            return WEDGE_ANGLE_FRACTION;
        }

        private void EnsureRoot()
        {
            if (_root != null)
                return;

            GameObject rootObject = new GameObject("PlantPlacementPreviewRoot");
            _root = rootObject.transform;
        }

        private static GameObject CreatePlacementMarker(Vector3 worldPosition, bool showCheck)
        {
            GameObject markerObject = new GameObject(showCheck ? "PlantPlacementAllowed" : "PlantPlacementDenied");
            markerObject.transform.SetPositionAndRotation(worldPosition, Quaternion.Euler(90f, 0f, 0f));

            Sprite markerSprite = showCheck ? GetOrCreateCheckSprite() : GetOrCreateCrossSprite();
            Color markerColor = showCheck ? CHECK_COLOR : CROSS_COLOR;

            CreateWorldSpriteChild(
                markerObject.transform,
                "Marker",
                markerSprite,
                Vector3.zero,
                MARKER_WORLD_SIZE,
                markerColor,
                MARKER_SORTING_ORDER,
                showCheck ? CHECK_SPRITE_Z_ROTATION : 0f);

            return markerObject;
        }

        private static GameObject CreateWorldSpriteChild(
            Transform parent,
            string objectName,
            Sprite sprite,
            Vector3 localPosition,
            float targetWorldSize,
            Color color,
            int sortingOrder,
            float localZRotationDegrees = 0f)
        {
            GameObject spriteObject = new GameObject(objectName);
            spriteObject.transform.SetParent(parent, false);
            spriteObject.transform.localPosition = localPosition;
            spriteObject.transform.localRotation = Quaternion.Euler(0f, 0f, localZRotationDegrees);
            spriteObject.transform.localScale = Vector3.one * GetScaleForWorldSize(sprite, targetWorldSize);

            SpriteRenderer spriteRenderer = spriteObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprite;
            spriteRenderer.color = color;
            spriteRenderer.sortingOrder = sortingOrder;
            spriteRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            spriteRenderer.receiveShadows = false;
            return spriteObject;
        }

        private static float GetScaleForWorldSize(Sprite sprite, float targetWorldSize)
        {
            float spriteWidth = sprite.bounds.size.x;

            if (spriteWidth <= 0.0001f)
                return targetWorldSize;

            return targetWorldSize / spriteWidth;
        }

        private static Sprite GetOrCreateCheckSprite()
        {
            if (_checkSprite != null)
                return _checkSprite;

            _checkSprite = CreateLineSprite(
                textureSize: 32,
                pixelsPerUnit: 32f,
                drawPixel: (int x, int y) => IsCheckPixel(x, y));
            return _checkSprite;
        }

        private static Sprite GetOrCreateCrossSprite()
        {
            if (_crossSprite != null)
                return _crossSprite;

            _crossSprite = CreateLineSprite(
                textureSize: 32,
                pixelsPerUnit: 32f,
                drawPixel: (int x, int y) => IsCrossPixel(x, y));
            return _crossSprite;
        }

        private static Sprite CreateLineSprite(int textureSize, float pixelsPerUnit, Func<int, int, bool> drawPixel)
        {
            Texture2D texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;

            Color32 transparent = new Color32(0, 0, 0, 0);
            Color32 white = new Color32(255, 255, 255, 255);

            for (int y = 0; y < textureSize; y++)
            {
                for (int x = 0; x < textureSize; x++)
                    texture.SetPixel(x, y, drawPixel(x, y) ? white : transparent);
            }

            texture.Apply();

            return Sprite.Create(
                texture,
                new Rect(0f, 0f, textureSize, textureSize),
                new Vector2(0.5f, 0.5f),
                pixelsPerUnit);
        }

        private static bool IsCheckPixel(int x, int y)
        {
            bool shortLeg = IsThickLine(x, y, 7, 15, 15, 7, 2);
            bool longLeg = IsThickLine(x, y, 14, 8, 24, 22, 2);

            return shortLeg || longLeg;
        }

        private static bool IsCrossPixel(int x, int y)
        {
            bool firstDiagonal = IsThickLine(x, y, 8, 24, 24, 8, 2);
            bool secondDiagonal = IsThickLine(x, y, 8, 8, 24, 24, 2);

            return firstDiagonal || secondDiagonal;
        }

        private static bool IsThickLine(
            int x,
            int y,
            int startX,
            int startY,
            int endX,
            int endY,
            int thickness)
        {
            float lineX = endX - startX;
            float lineY = endY - startY;
            float lineLengthSqr = lineX * lineX + lineY * lineY;

            if (lineLengthSqr <= 0.0001f)
                return false;

            float projection = ((x - startX) * lineX + (y - startY) * lineY) / lineLengthSqr;
            float closestX = startX + projection * lineX;
            float closestY = startY + projection * lineY;
            float distanceSqr = (x - closestX) * (x - closestX) + (y - closestY) * (y - closestY);
            float halfThickness = thickness * 0.5f;

            if (projection < 0f || projection > 1f)
                return false;

            return distanceSqr <= halfThickness * halfThickness;
        }
    }
}
