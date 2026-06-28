using System.Collections.Generic;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Sectors;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Stages;
using Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature
{
    public class SpawnPathPreviewService
    {
        private const float MARKER_GROUND_Y_OFFSET = 0.55f;
        private const float ICON_EXTRA_Y_OFFSET = 0.2f;
        private const float ARROW_WORLD_SIZE = 14f;
        private const float ENEMY_ICON_WORLD_SIZE = 14f;
        private const float ICON_OFFSET_TOWARD_CENTER = 11f;
        private const float ICON_ROW_SPACING = 10f;
        private const int ARROW_SORTING_ORDER = 150;
        private const int ICON_BACKING_SORTING_ORDER = 160;
        private const int ICON_SORTING_ORDER = 170;

        private static readonly Color ICON_BACKING_COLOR = new Color(0.05f, 0.05f, 0.08f, 0.75f);

        private readonly List<GameObject> _markerInstances = new();
        private Transform _root;

        public void Refresh(
            bool visible,
            SectorRegistryService registry,
            SectorVisualConfig visualConfig,
            WaveEnemyPreviewIconsConfig enemyIconsConfig,
            IReadOnlyList<SpawnGroupPlanEntry> groupPlans)
        {
            ClearMarkers();

            if (visible == false)
                return;

            if (registry == null || registry.IsInitialized == false)
                return;

            if (visualConfig == null || enemyIconsConfig == null)
                return;

            if (groupPlans == null || groupPlans.Count == 0)
                return;

            Sprite arrowSprite = visualConfig.SpawnPathArrowSprite;

            if (arrowSprite == null)
                return;

            EnsureRoot();

            Dictionary<int, List<WaveEnemyPreviewType>> threatsByPath = AggregateThreatsByPath(groupPlans);

            foreach (KeyValuePair<int, List<WaveEnemyPreviewType>> pathThreats in threatsByPath)
            {
                int pathIndex = pathThreats.Key;
                List<WaveEnemyPreviewType> previewTypes = pathThreats.Value;

                SectorId spawnSectorId = new SectorId(SectorBelt.Spawn, pathIndex);
                Vector3 anchor = registry.GetAnchorPosition(spawnSectorId);
                Vector3 toCenter = registry.Center - anchor;
                toCenter.y = 0f;

                if (toCenter.sqrMagnitude <= 0.0001f)
                    continue;

                List<Sprite> enemySprites = new List<Sprite>(previewTypes.Count);

                for (int typeIndex = 0; typeIndex < previewTypes.Count; typeIndex++)
                    enemySprites.Add(enemyIconsConfig.GetSpriteFor(previewTypes[typeIndex]));

                Vector3 inwardDirection = toCenter.normalized;
                GameObject marker = CreateThreatMarker(
                    anchor,
                    inwardDirection,
                    arrowSprite,
                    enemySprites,
                    visualConfig.SpawnPreviewOutlineColor);

                marker.transform.SetParent(_root, false);
                _markerInstances.Add(marker);
            }
        }

        public void ClearMarkers()
        {
            for (int index = 0; index < _markerInstances.Count; index++)
                Object.Destroy(_markerInstances[index]);

            _markerInstances.Clear();
        }

        private void EnsureRoot()
        {
            if (_root != null)
                return;

            GameObject rootObject = new GameObject("SpawnPathPreviewRoot");
            _root = rootObject.transform;
        }

        private static Dictionary<int, List<WaveEnemyPreviewType>> AggregateThreatsByPath(
            IReadOnlyList<SpawnGroupPlanEntry> groupPlans)
        {
            Dictionary<int, List<WaveEnemyPreviewType>> threatsByPath = new();

            for (int planIndex = 0; planIndex < groupPlans.Count; planIndex++)
            {
                SpawnGroupPlanEntry planEntry = groupPlans[planIndex];

                if (threatsByPath.TryGetValue(planEntry.PathIndex, out List<WaveEnemyPreviewType> previewTypes) == false)
                {
                    previewTypes = new List<WaveEnemyPreviewType>();
                    threatsByPath.Add(planEntry.PathIndex, previewTypes);
                }

                previewTypes.Add(planEntry.PreviewType);
            }

            return threatsByPath;
        }

        private static GameObject CreateThreatMarker(
            Vector3 spawnAnchor,
            Vector3 inwardDirection,
            Sprite arrowSprite,
            IReadOnlyList<Sprite> enemySprites,
            Color arrowTintColor)
        {
            GameObject markerObject = new GameObject("SpawnThreatMarker");

            float yRotationDegrees = Mathf.Atan2(inwardDirection.x, inwardDirection.z) * Mathf.Rad2Deg;
            Quaternion groundRotation = Quaternion.Euler(90f, yRotationDegrees, 0f);
            Vector3 rowDirection = Vector3.Cross(Vector3.up, inwardDirection).normalized;

            Vector3 arrowWorldPosition = spawnAnchor + Vector3.up * MARKER_GROUND_Y_OFFSET;
            Vector3 iconsRowCenter = arrowWorldPosition
                                     + inwardDirection * ICON_OFFSET_TOWARD_CENTER
                                     + Vector3.up * ICON_EXTRA_Y_OFFSET;

            markerObject.transform.SetPositionAndRotation(arrowWorldPosition, groundRotation);

            CreateWorldSpriteChild(
                markerObject.transform,
                "Arrow",
                arrowSprite,
                Vector3.zero,
                ARROW_WORLD_SIZE,
                arrowTintColor,
                ARROW_SORTING_ORDER);

            float rowStartOffset = (enemySprites.Count - 1) * 0.5f * ICON_ROW_SPACING;

            for (int iconIndex = 0; iconIndex < enemySprites.Count; iconIndex++)
            {
                Sprite enemySprite = enemySprites[iconIndex];
                Vector3 iconWorldPosition = iconsRowCenter
                                              + rowDirection * (iconIndex * ICON_ROW_SPACING - rowStartOffset);
                Vector3 iconLocalPosition = markerObject.transform.InverseTransformPoint(iconWorldPosition);
                int sortingOffset = iconIndex * 2;

                CreateWorldSpriteChild(
                    markerObject.transform,
                    $"IconBacking_{iconIndex}",
                    enemySprite,
                    iconLocalPosition,
                    ENEMY_ICON_WORLD_SIZE * 1.12f,
                    ICON_BACKING_COLOR,
                    ICON_BACKING_SORTING_ORDER + sortingOffset);

                CreateWorldSpriteChild(
                    markerObject.transform,
                    $"EnemyIcon_{iconIndex}",
                    enemySprite,
                    iconLocalPosition,
                    ENEMY_ICON_WORLD_SIZE,
                    Color.white,
                    ICON_SORTING_ORDER + sortingOffset);
            }

            return markerObject;
        }

        private static void CreateWorldSpriteChild(
            Transform parent,
            string objectName,
            Sprite sprite,
            Vector3 localPosition,
            float targetWorldSize,
            Color color,
            int sortingOrder)
        {
            GameObject spriteObject = new GameObject(objectName);
            spriteObject.transform.SetParent(parent, false);
            spriteObject.transform.localPosition = localPosition;
            spriteObject.transform.localRotation = Quaternion.identity;
            spriteObject.transform.localScale = Vector3.one * GetScaleForWorldSize(sprite, targetWorldSize);

            SpriteRenderer spriteRenderer = spriteObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprite;
            spriteRenderer.color = color;
            spriteRenderer.sortingOrder = sortingOrder;
            spriteRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            spriteRenderer.receiveShadows = false;
        }

        private static float GetScaleForWorldSize(Sprite sprite, float targetWorldSize)
        {
            float spriteWidth = sprite.bounds.size.x;

            if (spriteWidth <= 0.0001f)
                return targetWorldSize;

            return targetWorldSize / spriteWidth;
        }
    }
}
