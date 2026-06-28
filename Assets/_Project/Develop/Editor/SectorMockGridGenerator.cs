using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Sectors;
using Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets._Project.Develop.Editor
{
    public static class SectorMockGridGenerator
    {
        private const string GENERATED_ROOT_NAME = "GeneratedVolumes";
        private const string OUTLINE_OBJECT_NAME = "Outline";
        private const string SECTOR_GRID_OBJECT_NAME = "SectorGrid";
        private const string TOWER_CONFIG_PATH = "Configs/Gameplay/Entities/Characters/TowerConfig";
        private const string SECTOR_GRID_CONFIG_PATH = "Configs/Gameplay/Sectors/SectorGridConfig";
        private const string SECTOR_VISUAL_CONFIG_PATH = "Configs/Gameplay/Sectors/SectorVisualConfig";
        private const float GRID_VISUAL_Y_OFFSET = 0.5f;
        private const float WEDGE_MESH_THICKNESS = 0.04f;
        private const float OUTLINE_Y_OFFSET = 0.03f;
        private const float FULL_CIRCLE_RADIANS = Mathf.PI * 2f;

        [MenuItem("Tools/Sectors/Generate Mock Sector Grid")]
        public static void Generate()
        {
            TowerConfig towerConfig = LoadConfig<TowerConfig>(TOWER_CONFIG_PATH);
            SectorGridConfig gridConfig = LoadConfig<SectorGridConfig>(SECTOR_GRID_CONFIG_PATH);
            SectorVisualConfig visualConfig = LoadConfig<SectorVisualConfig>(SECTOR_VISUAL_CONFIG_PATH);

            if (towerConfig == null || gridConfig == null || visualConfig == null)
                return;

            SectorBootstrap bootstrap = FindOrCreateBootstrap();
            Transform generatedRoot = FindOrCreateGeneratedRoot(bootstrap.transform);

            ClearGeneratedVolumes(generatedRoot);

            Undo.RecordObject(bootstrap.transform, "Generate Mock Sector Grid");

            Vector3 gridPosition = towerConfig.StartPosition;
            gridPosition.y += GRID_VISUAL_Y_OFFSET;
            bootstrap.transform.position = gridPosition;

            for (int beltIndex = 0; beltIndex < 4; beltIndex++)
            {
                SectorBelt belt = (SectorBelt)beltIndex;

                for (int sectorIndex = 0; sectorIndex < SectorId.SectorsPerRing; sectorIndex++)
                    CreateWedge(generatedRoot, belt, sectorIndex, gridConfig, visualConfig);
            }

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Selection.activeGameObject = bootstrap.gameObject;

            Debug.Log($"Mock sector grid generated: {SectorId.SectorsPerRing * 4} volumes under '{bootstrap.name}/{GENERATED_ROOT_NAME}'.");
        }

        [MenuItem("Tools/Sectors/Clear Mock Sector Grid")]
        public static void Clear()
        {
            SectorBootstrap bootstrap = Object.FindObjectOfType<SectorBootstrap>();

            if (bootstrap == null)
            {
                Debug.Log("SectorBootstrap not found in scene.");
                return;
            }

            Transform generatedRoot = bootstrap.transform.Find(GENERATED_ROOT_NAME);

            if (generatedRoot == null)
            {
                Debug.Log("Generated sector volumes not found.");
                return;
            }

            Undo.DestroyObjectImmediate(generatedRoot.gameObject);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

            Debug.Log("Mock sector grid cleared.");
        }

        private static T LoadConfig<T>(string resourcesPath) where T : Object
        {
            T config = Resources.Load<T>(resourcesPath);

            if (config == null)
                Debug.LogError($"Config not found at Resources/{resourcesPath}");

            return config;
        }

        private static SectorBootstrap FindOrCreateBootstrap()
        {
            SectorBootstrap bootstrap = Object.FindObjectOfType<SectorBootstrap>();

            if (bootstrap != null)
                return bootstrap;

            GameObject bootstrapObject = new GameObject(SECTOR_GRID_OBJECT_NAME);
            bootstrap = bootstrapObject.AddComponent<SectorBootstrap>();
            Undo.RegisterCreatedObjectUndo(bootstrapObject, "Create SectorGrid");

            return bootstrap;
        }

        private static Transform FindOrCreateGeneratedRoot(Transform bootstrapTransform)
        {
            Transform existingRoot = bootstrapTransform.Find(GENERATED_ROOT_NAME);

            if (existingRoot != null)
                return existingRoot;

            GameObject rootObject = new GameObject(GENERATED_ROOT_NAME);
            Undo.RegisterCreatedObjectUndo(rootObject, "Create GeneratedVolumes");

            rootObject.transform.SetParent(bootstrapTransform, false);
            rootObject.transform.localPosition = Vector3.zero;
            rootObject.transform.localRotation = Quaternion.identity;
            rootObject.transform.localScale = Vector3.one;

            return rootObject.transform;
        }

        private static void ClearGeneratedVolumes(Transform generatedRoot)
        {
            for (int childIndex = generatedRoot.childCount - 1; childIndex >= 0; childIndex--)
                Undo.DestroyObjectImmediate(generatedRoot.GetChild(childIndex).gameObject);
        }

        private static void CreateWedge(
            Transform parent,
            SectorBelt belt,
            int sectorIndex,
            SectorGridConfig gridConfig,
            SectorVisualConfig visualConfig)
        {
            GetBeltRadii(belt, gridConfig, out float innerRadius, out float outerRadius);

            float sectorWidthRadians = FULL_CIRCLE_RADIANS / SectorId.SectorsPerRing;
            float startAngleRadians = sectorIndex * sectorWidthRadians;
            float endAngleRadians = startAngleRadians + sectorWidthRadians;
            float midAngleRadians = startAngleRadians + sectorWidthRadians * 0.5f;
            float anchorRadius = GetAnchorRadius(belt, gridConfig);

            Vector3 anchorLocalPosition = ToXZ(midAngleRadians, anchorRadius);

            GameObject wedgeObject = new GameObject($"{belt}_{sectorIndex}");
            Undo.RegisterCreatedObjectUndo(wedgeObject, "Create Sector Wedge");

            wedgeObject.transform.SetParent(parent, false);
            wedgeObject.transform.localPosition = anchorLocalPosition;
            wedgeObject.transform.localRotation = Quaternion.identity;
            wedgeObject.transform.localScale = Vector3.one;

            Vector3[] outlinePoints = BuildWedgeOutlinePoints(
                innerRadius,
                outerRadius,
                startAngleRadians,
                endAngleRadians,
                anchorLocalPosition);

            Mesh wedgeMesh = BuildWedgeMesh(outlinePoints, innerRadius <= 0.001f);
            wedgeMesh.name = $"{belt}_{sectorIndex}_Mesh";

            MeshFilter meshFilter = wedgeObject.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = wedgeMesh;

            SectorFillVisualData fillVisual = visualConfig.UnlockedFill;
            Material wedgeMaterial = SectorVisualUtility.CreateTransparentFillMaterial(fillVisual.Color, fillVisual.Alpha);

            MeshRenderer meshRenderer = wedgeObject.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = wedgeMaterial;

            BoxCollider boxCollider = wedgeObject.AddComponent<BoxCollider>();
            Bounds meshBounds = wedgeMesh.bounds;
            boxCollider.center = meshBounds.center;
            boxCollider.size = new Vector3(meshBounds.size.x, WEDGE_MESH_THICKNESS, meshBounds.size.z);
            boxCollider.isTrigger = true;

            LineRenderer outline = CreateOutline(wedgeObject.transform, outlinePoints, visualConfig);

            SectorVolumeRegistrator registrator = wedgeObject.AddComponent<SectorVolumeRegistrator>();
            SectorView sectorView = wedgeObject.AddComponent<SectorView>();

            SerializedObject registratorSerializedObject = new SerializedObject(registrator);
            registratorSerializedObject.FindProperty("_belt").enumValueIndex = (int)belt;
            registratorSerializedObject.FindProperty("_index").intValue = sectorIndex;
            registratorSerializedObject.FindProperty("_triggerCollider").objectReferenceValue = boxCollider;
            registratorSerializedObject.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject viewSerializedObject = new SerializedObject(sectorView);
            viewSerializedObject.FindProperty("_fillRenderer").objectReferenceValue = meshRenderer;
            viewSerializedObject.FindProperty("_outline").objectReferenceValue = outline;
            viewSerializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static LineRenderer CreateOutline(
            Transform wedgeTransform,
            Vector3[] outlinePoints,
            SectorVisualConfig visualConfig)
        {
            GameObject outlineObject = new GameObject(OUTLINE_OBJECT_NAME);
            Undo.RegisterCreatedObjectUndo(outlineObject, "Create Sector Outline");

            outlineObject.transform.SetParent(wedgeTransform, false);
            outlineObject.transform.localPosition = Vector3.zero;
            outlineObject.transform.localRotation = Quaternion.identity;
            outlineObject.transform.localScale = Vector3.one;

            LineRenderer lineRenderer = outlineObject.AddComponent<LineRenderer>();
            lineRenderer.positionCount = outlinePoints.Length;

            for (int pointIndex = 0; pointIndex < outlinePoints.Length; pointIndex++)
            {
                Vector3 point = outlinePoints[pointIndex];
                point.y = OUTLINE_Y_OFFSET;
                lineRenderer.SetPosition(pointIndex, point);
            }

            SectorVisualUtility.ConfigureOutline(lineRenderer, visualConfig);

            return lineRenderer;
        }

        private static Vector3[] BuildWedgeOutlinePoints(
            float innerRadius,
            float outerRadius,
            float startAngleRadians,
            float endAngleRadians,
            Vector3 anchorLocalPosition)
        {
            if (innerRadius <= 0.001f)
            {
                Vector3 center = Vector3.zero - anchorLocalPosition;
                Vector3 triangleOuterStart = ToXZ(startAngleRadians, outerRadius) - anchorLocalPosition;
                Vector3 triangleOuterEnd = ToXZ(endAngleRadians, outerRadius) - anchorLocalPosition;

                return new[]
                {
                    center,
                    triangleOuterEnd,
                    triangleOuterStart,
                };
            }

            Vector3 innerStart = ToXZ(startAngleRadians, innerRadius) - anchorLocalPosition;
            Vector3 innerEnd = ToXZ(endAngleRadians, innerRadius) - anchorLocalPosition;
            Vector3 outerEnd = ToXZ(endAngleRadians, outerRadius) - anchorLocalPosition;
            Vector3 outerStart = ToXZ(startAngleRadians, outerRadius) - anchorLocalPosition;

            return new[]
            {
                innerStart,
                innerEnd,
                outerEnd,
                outerStart,
            };
        }

        private static Mesh BuildWedgeMesh(Vector3[] outlinePoints, bool isTriangle)
        {
            Mesh mesh = new Mesh();
            mesh.vertices = outlinePoints;

            if (isTriangle)
                mesh.triangles = new[] { 0, 1, 2 };
            else
                mesh.triangles = new[] { 0, 1, 2, 0, 2, 3 };

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }

        private static void GetBeltRadii(SectorBelt belt, SectorGridConfig gridConfig, out float innerRadius, out float outerRadius)
        {
            switch (belt)
            {
                case SectorBelt.Inner:
                    innerRadius = 0f;
                    outerRadius = gridConfig.InnerBeltMaxRadius;
                    break;

                case SectorBelt.Middle:
                    innerRadius = gridConfig.InnerBeltMaxRadius;
                    outerRadius = gridConfig.MiddleBeltMaxRadius;
                    break;

                case SectorBelt.Outer:
                    innerRadius = gridConfig.MiddleBeltMaxRadius;
                    outerRadius = gridConfig.OuterBeltMaxRadius;
                    break;

                case SectorBelt.Spawn:
                    innerRadius = gridConfig.OuterBeltMaxRadius;
                    outerRadius = gridConfig.SpawnBeltMaxRadius;
                    break;

                default:
                    innerRadius = 0f;
                    outerRadius = gridConfig.SpawnBeltMaxRadius;
                    break;
            }
        }

        private static float GetAnchorRadius(SectorBelt belt, SectorGridConfig gridConfig)
        {
            switch (belt)
            {
                case SectorBelt.Inner:
                    return gridConfig.InnerBeltAnchorRadius;

                case SectorBelt.Middle:
                    return gridConfig.MiddleBeltAnchorRadius;

                case SectorBelt.Outer:
                    return gridConfig.OuterBeltAnchorRadius;

                case SectorBelt.Spawn:
                    return gridConfig.SpawnBeltAnchorRadius;

                default:
                    return gridConfig.SpawnBeltAnchorRadius;
            }
        }

        private static Vector3 ToXZ(float angleRadians, float radius)
        {
            return new Vector3(Mathf.Cos(angleRadians) * radius, 0f, Mathf.Sin(angleRadians) * radius);
        }
    }
}
