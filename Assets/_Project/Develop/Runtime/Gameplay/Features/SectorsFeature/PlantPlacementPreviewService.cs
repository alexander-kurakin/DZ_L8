using System;
using System.Collections.Generic;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
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
        private const float SELL_MARKER_OUTWARD_OFFSET = 8f;
        private const float SELL_MARKER_OUTER_EDGE_MARGIN = 1f;
        private const float MARKER_WORLD_SIZE = 7f;
        private const float CHECK_SPRITE_Z_ROTATION = -90f;
        private const int MARKER_SORTING_ORDER = 175;
        private const float HOVER_CROSS_APPEAR_DURATION_SECONDS = 0.18f;
        private const float HOVER_CROSS_SCALE_FROM = 0.55f;
        private const float SELL_SHOVEL_HIT_RADIUS_FRACTION = 0.32f;
        private const float SHOVEL_OUTLINE_WORLD_SIZE = MARKER_WORLD_SIZE * 0.88f;
        private const int SHOVEL_OUTLINE_SORTING_ORDER = MARKER_SORTING_ORDER - 1;

        private static readonly Color CHECK_COLOR = new Color(0.25f, 0.95f, 0.35f, 1f);
        private static readonly Color TOXIC_OUTER_RECOMMENDED_COLOR = new Color(0.2f, 1f, 0.45f, 1f);
        private static readonly Color CROSS_COLOR = new Color(0.95f, 0.25f, 0.25f, 1f);
        private static readonly Color SHOVEL_COLOR = new Color(0.85f, 0.62f, 0.18f, 1f);
        private static readonly Color SHOVEL_OUTLINE_IDLE_COLOR = new Color(1f, 1f, 1f, 0.35f);
        private static readonly Color SHOVEL_OUTLINE_HOVER_COLOR = new Color(1f, 1f, 1f, 1f);

        private static readonly SectorBelt[] PLANTABLE_BELTS =
        {
            SectorBelt.Inner,
            SectorBelt.Middle,
            SectorBelt.Outer,
        };

        private static Sprite _checkSprite;
        private static Sprite _crossSprite;
        private static Sprite _shovelSprite;
        private static Sprite _rectOutlineSprite;

        private readonly Dictionary<SectorId, GameObject> _markerBySectorId = new();
        private Transform _root;
        private GameObject _hoverCrossMarker;
        private Transform _hoverCrossVisual;
        private SpriteRenderer _hoverCrossRenderer;
        private float _hoverCrossBaseScale = 1f;
        private SectorId? _hoveredSectorId;
        private Tween _hoverCrossTween;
        private GameObject _hoverSellShovelMarker;
        private Transform _hoverSellShovelVisual;
        private SpriteRenderer _hoverSellShovelRenderer;
        private SpriteRenderer _hoverSellShovelOutlineRenderer;
        private float _hoverSellShovelBaseScale = 1f;
        private SectorId? _hoveredSellSectorId;
        private Tween _hoverSellShovelTween;

        public bool HasSellHoverTarget => _hoveredSellSectorId.HasValue;

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

            ClearHoverCross();
        }

        public void UpdateHoverSellShovel(
            SectorId hoveredSectorId,
            SectorRegistryService registry,
            PlantPlacementService plantPlacementService)
        {
            if (registry == null || registry.IsInitialized == false || plantPlacementService == null)
            {
                ClearHoverSellShovel();
                return;
            }

            if (hoveredSectorId.Belt == SectorBelt.Spawn)
            {
                ClearHoverSellShovel();
                return;
            }

            if (plantPlacementService.TryGetPlantAtSector(hoveredSectorId, out Entity plantEntity) == false
                || plantEntity == null)
            {
                ClearHoverSellShovel();
                return;
            }

            if (_hoveredSellSectorId.HasValue && _hoveredSellSectorId.Value == hoveredSectorId)
                return;

            _hoveredSellSectorId = hoveredSectorId;
            Vector3 markerPosition = GetSellMarkerPosition(registry, hoveredSectorId);
            ShowHoverSellShovel(markerPosition);
        }

        public bool TryGetSellHoverSector(out SectorId sectorId)
        {
            if (_hoveredSellSectorId.HasValue)
            {
                sectorId = _hoveredSellSectorId.Value;
                return true;
            }

            sectorId = default;
            return false;
        }

        public bool IsPointerOverActiveSellShovel(Vector3 clickWorldPoint)
        {
            if (_hoverSellShovelMarker == null || _hoverSellShovelMarker.activeSelf == false)
                return false;

            if (_hoveredSellSectorId.HasValue == false)
                return false;

            Vector3 shovelPosition = _hoverSellShovelMarker.transform.position;
            float deltaX = clickWorldPoint.x - shovelPosition.x;
            float deltaZ = clickWorldPoint.z - shovelPosition.z;
            float hitRadius = MARKER_WORLD_SIZE * SELL_SHOVEL_HIT_RADIUS_FRACTION;

            return deltaX * deltaX + deltaZ * deltaZ <= hitRadius * hitRadius;
        }

        public void UpdateActiveSellShovelPointerHighlight(Vector3 planeWorldPoint)
        {
            if (_hoverSellShovelMarker == null || _hoverSellShovelMarker.activeSelf == false)
                return;

            if (_hoverSellShovelOutlineRenderer == null)
                return;

            bool isPointerOverShovel = IsPointerOverActiveSellShovel(planeWorldPoint);
            _hoverSellShovelOutlineRenderer.color = isPointerOverShovel
                ? SHOVEL_OUTLINE_HOVER_COLOR
                : SHOVEL_OUTLINE_IDLE_COLOR;
        }

        public void ClearHoverSellShovel()
        {
            _hoverSellShovelTween?.Kill();
            _hoverSellShovelTween = null;
            _hoveredSellSectorId = null;

            if (_hoverSellShovelMarker != null)
                _hoverSellShovelMarker.SetActive(false);
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
            ClearHoverSellShovel();

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

            Vector3 markerPosition = GetPlantMarkerPosition(registry, sectorId);
            bool showCheck = previewState == PlantPlacementPreviewState.Allowed;
            GameObject marker = CreatePlacementMarker(markerPosition, showCheck, abilityType, sectorId.Belt);
            marker.transform.SetParent(_root, false);
            _markerBySectorId[sectorId] = marker;
        }

        private static Vector3 GetPlantMarkerPosition(SectorRegistryService registry, SectorId sectorId)
        {
            Vector3 anchor = registry.GetAnchorPosition(sectorId);

            return new Vector3(anchor.x, anchor.y + MARKER_GROUND_Y_OFFSET, anchor.z);
        }

        private static Vector3 GetSellMarkerPosition(SectorRegistryService registry, SectorId sectorId)
        {
            Vector3 plantAnchor = registry.GetAnchorPosition(sectorId);
            Vector3 fromCenter = plantAnchor - registry.Center;
            fromCenter.y = 0f;

            if (fromCenter.sqrMagnitude <= 0.0001f)
                return GetPlantMarkerPosition(registry, sectorId);

            Vector3 outward = fromCenter.normalized;
            float plantRadius = fromCenter.magnitude;
            float outerRadius = WorldToSector.GetBeltOuterRadius(sectorId.Belt, registry.GridConfig);
            float maxOutward = Mathf.Max(0f, outerRadius - plantRadius - SELL_MARKER_OUTER_EDGE_MARGIN);
            float outwardOffset = Mathf.Min(SELL_MARKER_OUTWARD_OFFSET, maxOutward);
            Vector3 sellPosition = plantAnchor + outward * outwardOffset;

            return new Vector3(
                sellPosition.x,
                registry.Center.y + MARKER_GROUND_Y_OFFSET,
                sellPosition.z);
        }

        private void ShowHoverSellShovel(Vector3 markerPosition)
        {
            EnsureRoot();
            EnsureHoverSellShovelMarker();
            _hoverSellShovelMarker.transform.SetPositionAndRotation(markerPosition, Quaternion.Euler(90f, 0f, 0f));
            _hoverSellShovelMarker.SetActive(true);

            _hoverSellShovelTween?.Kill();
            _hoverSellShovelVisual.localScale = Vector3.one * (_hoverSellShovelBaseScale * HOVER_CROSS_SCALE_FROM);

            Color hiddenColor = SHOVEL_COLOR;
            hiddenColor.a = 0f;
            _hoverSellShovelRenderer.color = hiddenColor;

            if (_hoverSellShovelOutlineRenderer != null)
                _hoverSellShovelOutlineRenderer.color = SHOVEL_OUTLINE_IDLE_COLOR;

            _hoverSellShovelTween = DOTween.Sequence()
                .Append(_hoverSellShovelVisual
                    .DOScale(Vector3.one * _hoverSellShovelBaseScale, HOVER_CROSS_APPEAR_DURATION_SECONDS)
                    .SetEase(Ease.OutBack))
                .Join(_hoverSellShovelRenderer
                    .DOColor(SHOVEL_COLOR, HOVER_CROSS_APPEAR_DURATION_SECONDS)
                    .SetEase(Ease.OutQuad))
                .Play();
        }

        private void EnsureHoverSellShovelMarker()
        {
            if (_hoverSellShovelMarker != null)
                return;

            _hoverSellShovelMarker = new GameObject("PlantSellHoverShovel");
            _hoverSellShovelMarker.transform.SetParent(_root, false);
            _hoverSellShovelMarker.transform.SetPositionAndRotation(Vector3.zero, Quaternion.Euler(90f, 0f, 0f));

            Sprite outlineSprite = GetOrCreateRectOutlineSprite();
            GameObject outlineObject = CreateWorldSpriteChild(
                _hoverSellShovelMarker.transform,
                "Outline",
                outlineSprite,
                Vector3.zero,
                SHOVEL_OUTLINE_WORLD_SIZE,
                SHOVEL_OUTLINE_IDLE_COLOR,
                SHOVEL_OUTLINE_SORTING_ORDER);
            _hoverSellShovelOutlineRenderer = outlineObject.GetComponent<SpriteRenderer>();

            Sprite shovelSprite = GetOrCreateShovelSprite();
            _hoverSellShovelBaseScale = GetScaleForWorldSize(shovelSprite, MARKER_WORLD_SIZE);
            _hoverSellShovelVisual = CreateWorldSpriteChild(
                _hoverSellShovelMarker.transform,
                "Marker",
                shovelSprite,
                Vector3.zero,
                MARKER_WORLD_SIZE,
                SHOVEL_COLOR,
                MARKER_SORTING_ORDER).transform;
            _hoverSellShovelRenderer = _hoverSellShovelVisual.GetComponent<SpriteRenderer>();
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

        private void EnsureRoot()
        {
            if (_root != null)
                return;

            GameObject rootObject = new GameObject("PlantPlacementPreviewRoot");
            _root = rootObject.transform;
        }

        private static GameObject CreatePlacementMarker(
            Vector3 worldPosition,
            bool showCheck,
            AbilityType abilityType,
            SectorBelt belt)
        {
            GameObject markerObject = new GameObject(showCheck ? "PlantPlacementAllowed" : "PlantPlacementDenied");
            markerObject.transform.SetPositionAndRotation(worldPosition, Quaternion.Euler(90f, 0f, 0f));

            Sprite markerSprite = showCheck ? GetOrCreateCheckSprite() : GetOrCreateCrossSprite();
            Color markerColor = ResolveMarkerColor(showCheck, abilityType, belt);
            float markerWorldSize = ResolveMarkerWorldSize(showCheck, abilityType, belt);

            CreateWorldSpriteChild(
                markerObject.transform,
                "Marker",
                markerSprite,
                Vector3.zero,
                markerWorldSize,
                markerColor,
                MARKER_SORTING_ORDER,
                showCheck ? CHECK_SPRITE_Z_ROTATION : 0f);

            return markerObject;
        }

        private static Color ResolveMarkerColor(bool showCheck, AbilityType abilityType, SectorBelt belt)
        {
            if (showCheck == false)
                return CROSS_COLOR;

            if (abilityType == AbilityType.PlantToxicArea && belt == SectorBelt.Outer)
                return TOXIC_OUTER_RECOMMENDED_COLOR;

            return CHECK_COLOR;
        }

        private static float ResolveMarkerWorldSize(bool showCheck, AbilityType abilityType, SectorBelt belt)
        {
            if (showCheck
                && abilityType == AbilityType.PlantToxicArea
                && belt == SectorBelt.Outer)
            {
                return MARKER_WORLD_SIZE * 1.15f;
            }

            return MARKER_WORLD_SIZE;
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

        private static Sprite GetOrCreateShovelSprite()
        {
            if (_shovelSprite != null)
                return _shovelSprite;

            _shovelSprite = CreateLineSprite(
                textureSize: 32,
                pixelsPerUnit: 32f,
                drawPixel: (int x, int y) => IsShovelPixel(x, y));
            return _shovelSprite;
        }

        private static Sprite GetOrCreateRectOutlineSprite()
        {
            if (_rectOutlineSprite != null)
                return _rectOutlineSprite;

            _rectOutlineSprite = CreateLineSprite(
                textureSize: 32,
                pixelsPerUnit: 32f,
                drawPixel: (int x, int y) => IsRectOutlinePixel(x, y, borderThickness: 2));
            return _rectOutlineSprite;
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

        private static bool IsShovelPixel(int x, int y)
        {
            bool handle = IsThickLine(x, y, 16, 24, 16, 10, 2);
            bool blade = x >= 9 && x <= 23 && y >= 4 && y <= 11;
            bool bladeTip = IsThickLine(x, y, 10, 4, 22, 4, 2);

            return handle || blade || bladeTip;
        }

        private static bool IsRectOutlinePixel(int x, int y, int borderThickness)
        {
            const int textureSize = 32;
            int innerMin = borderThickness;
            int innerMax = textureSize - 1 - borderThickness;
            bool inOuterRect = x >= 0 && x < textureSize && y >= 0 && y < textureSize;
            bool inInnerHole = x > innerMin && x < innerMax && y > innerMin && y < innerMax;

            return inOuterRect && inInnerHole == false;
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
