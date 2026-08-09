using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace Assets._Project.Develop.Editor
{
    public static class ParticleVfxIconBaker
    {
        private const string OUTPUT_FOLDER = "Assets/_Project/Art/UI/Sprites/Abilities";
        private const int RESOLUTION = 512;
        private const float BOUNDS_PADDING = 1.2f;
        private const float ALPHA_BOOST = 1.35f;

        private const float SIMULATE_STEP_SECONDS = 1f / 60f;

        private static readonly BakeJob[] Jobs =
        {
            new BakeJob(
                "Assets/_Project/Art/VFX/ToSortOutDependencies/Slash_fire_long.prefab",
                "ICON_Slash_fire_long.png",
                simulateSeconds: 2f,
                cameraEuler: new Vector3(90f, 0f, 0f)),
            new BakeJob(
                "Assets/_Project/Art/VFX/ToSortOutDependencies/Toxic_area.prefab",
                "ICON_Toxic_area.png",
                simulateSeconds: 1.1f,
                cameraEuler: new Vector3(90f, 0f, 0f)),
        };

        [MenuItem("Tools/VFX/Bake Ability Icons From VFX Prefabs")]
        public static void BakeAbilityIcons()
        {
            if (Directory.Exists(OUTPUT_FOLDER) == false)
                Directory.CreateDirectory(OUTPUT_FOLDER);

            int bakedCount = 0;

            foreach (BakeJob job in Jobs)
            {
                if (BakeJobToPng(job))
                    bakedCount++;
            }

            AssetDatabase.Refresh();
            Debug.Log($"[ParticleVfxIconBaker] Baked {bakedCount}/{Jobs.Length} icons into {OUTPUT_FOLDER}");
        }

        private static bool BakeJobToPng(BakeJob job)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(job.PrefabPath);

            if (prefab == null)
            {
                Debug.LogError($"[ParticleVfxIconBaker] Prefab not found: {job.PrefabPath}");
                return false;
            }

            Scene previewScene = default;
            GameObject instance = null;
            GameObject cameraObject = null;
            GameObject lightObject = null;
            RenderTexture renderTexture = null;
            Texture2D texture = null;

            Color previousAmbient = RenderSettings.ambientLight;
            AmbientMode previousAmbientMode = RenderSettings.ambientMode;
            bool previousFog = RenderSettings.fog;

            try
            {
                previewScene = EditorSceneManager.NewPreviewScene();

                if (EditorSceneManager.GetSceneCullingMask(previewScene) == 0)
                {
                    Debug.LogError("[ParticleVfxIconBaker] Too many preview scenes open. Restart Unity.");
                    return false;
                }

                instance = Object.Instantiate(prefab);
                instance.name = prefab.name + "_BakeInstance";
                instance.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                instance.transform.localScale = Vector3.one;
                SceneManager.MoveGameObjectToScene(instance, previewScene);

                cameraObject = new GameObject("BakeCamera");
                lightObject = new GameObject("BakeLight");
                SceneManager.MoveGameObjectToScene(cameraObject, previewScene);
                SceneManager.MoveGameObjectToScene(lightObject, previewScene);

                ParticleSystem[] particleSystems = instance.GetComponentsInChildren<ParticleSystem>(true);

                if (particleSystems.Length == 0)
                {
                    Debug.LogError($"[ParticleVfxIconBaker] No ParticleSystem on {job.PrefabPath}");
                    return false;
                }

                ParticleSystem rootParticleSystem = instance.GetComponent<ParticleSystem>();

                if (rootParticleSystem == null)
                    rootParticleSystem = particleSystems[0];

                PrepareLoopingLikeMineIdle(particleSystems);
                SimulateParticleHierarchy(rootParticleSystem, job.SimulateSeconds);

                Bounds bounds = CalculateParticleBounds(instance);

                if (bounds.size == Vector3.zero)
                    bounds = new Bounds(Vector3.zero, Vector3.one * 2f);

                Camera camera = cameraObject.AddComponent<Camera>();
                camera.scene = previewScene;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0f, 0f, 0f, 0f);
                camera.orthographic = true;
                camera.nearClipPlane = 0.01f;
                camera.farClipPlane = 1000f;
                camera.allowHDR = false;
                camera.allowMSAA = true;
                camera.depthTextureMode = DepthTextureMode.None;

                UniversalAdditionalCameraData cameraData = camera.GetUniversalAdditionalCameraData();
                cameraData.renderPostProcessing = false;
                cameraData.renderShadows = false;

                Light directionalLight = lightObject.AddComponent<Light>();
                directionalLight.type = LightType.Directional;
                directionalLight.color = Color.white;
                directionalLight.intensity = 1f;
                lightObject.transform.eulerAngles = new Vector3(50f, -30f, 0f);

                RenderSettings.ambientLight = Color.gray;
                RenderSettings.ambientMode = AmbientMode.Flat;
                RenderSettings.fog = false;

                float cameraDistance = Mathf.Max(bounds.extents.magnitude * 3f, 2f);
                camera.transform.rotation = Quaternion.Euler(job.CameraEuler);
                camera.transform.position = bounds.center - camera.transform.forward * cameraDistance;

                float halfExtentX = ProjectExtent(bounds, camera.transform.right);
                float halfExtentY = ProjectExtent(bounds, camera.transform.up);
                camera.orthographicSize = Mathf.Max(halfExtentX, halfExtentY, 0.1f) * BOUNDS_PADDING;

                UniversalRenderPipelineAsset urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
                int msaaSamples = urpAsset != null ? urpAsset.msaaSampleCount : 4;

                RenderTextureDescriptor descriptor = new RenderTextureDescriptor(RESOLUTION, RESOLUTION)
                {
                    depthBufferBits = 24,
                    msaaSamples = msaaSamples,
                    useMipMap = false,
                    sRGB = true,
                    colorFormat = RenderTextureFormat.ARGB32,
                };

                renderTexture = new RenderTexture(descriptor);
                renderTexture.Create();

                camera.targetTexture = renderTexture;
                camera.aspect = 1f;
                camera.Render();
                camera.targetTexture = null;

                RenderTexture previousActive = RenderTexture.active;
                RenderTexture.active = renderTexture;

                texture = new Texture2D(RESOLUTION, RESOLUTION, TextureFormat.RGBA32, false, false);
                texture.ReadPixels(new Rect(0, 0, RESOLUTION, RESOLUTION), 0, 0);
                texture.Apply(false, false);

                RenderTexture.active = previousActive;

                ApplyLuminanceAlpha(texture);

                string absoluteOutputPath = Path.Combine(Directory.GetCurrentDirectory(), OUTPUT_FOLDER, job.FileName)
                    .Replace('\\', '/');
                string assetPath = $"{OUTPUT_FOLDER}/{job.FileName}";

                File.WriteAllBytes(absoluteOutputPath, texture.EncodeToPNG());

                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                ConfigureAsAbilitySprite(assetPath);

                Debug.Log($"[ParticleVfxIconBaker] Saved {assetPath}");
                return true;
            }
            finally
            {
                RenderSettings.ambientLight = previousAmbient;
                RenderSettings.ambientMode = previousAmbientMode;
                RenderSettings.fog = previousFog;

                if (texture != null)
                    Object.DestroyImmediate(texture);

                if (renderTexture != null)
                {
                    renderTexture.Release();
                    Object.DestroyImmediate(renderTexture);
                }

                if (instance != null)
                    Object.DestroyImmediate(instance);

                if (cameraObject != null)
                    Object.DestroyImmediate(cameraObject);

                if (lightObject != null)
                    Object.DestroyImmediate(lightObject);

                if (previewScene.IsValid())
                    EditorSceneManager.ClosePreviewScene(previewScene);
            }
        }

        private static void PrepareLoopingLikeMineIdle(ParticleSystem[] particleSystems)
        {
            for (int index = 0; index < particleSystems.Length; index++)
            {
                ParticleSystem.MainModule mainModule = particleSystems[index].main;
                mainModule.loop = true;
                mainModule.prewarm = true;
                mainModule.scalingMode = ParticleSystemScalingMode.Hierarchy;
            }
        }

        private static void SimulateParticleHierarchy(ParticleSystem rootParticleSystem, float simulateSeconds)
        {
            rootParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            rootParticleSystem.Play(true);

            float simulatedSeconds = 0f;

            while (simulatedSeconds < simulateSeconds)
            {
                float stepSeconds = Mathf.Min(SIMULATE_STEP_SECONDS, simulateSeconds - simulatedSeconds);
                rootParticleSystem.Simulate(stepSeconds, true, false, true);
                simulatedSeconds += stepSeconds;
            }
        }

        private static Bounds CalculateParticleBounds(GameObject root)
        {
            ParticleSystemRenderer[] renderers = root.GetComponentsInChildren<ParticleSystemRenderer>(true);

            if (renderers.Length == 0)
                return new Bounds(root.transform.position, Vector3.one);

            Bounds bounds = default;
            bool hasBounds = false;

            for (int index = 0; index < renderers.Length; index++)
            {
                Bounds rendererBounds = renderers[index].bounds;

                if (rendererBounds.size == Vector3.zero)
                    continue;

                if (hasBounds == false)
                {
                    bounds = rendererBounds;
                    hasBounds = true;
                    continue;
                }

                bounds.Encapsulate(rendererBounds);
            }

            if (hasBounds == false)
                return new Bounds(root.transform.position, Vector3.one * 2f);

            return bounds;
        }

        private static float ProjectExtent(Bounds bounds, Vector3 axis)
        {
            Vector3 extents = bounds.extents;
            return Mathf.Abs(extents.x * axis.x)
                   + Mathf.Abs(extents.y * axis.y)
                   + Mathf.Abs(extents.z * axis.z);
        }

        private static void ApplyLuminanceAlpha(Texture2D texture)
        {
            Color32[] pixels = texture.GetPixels32();

            for (int index = 0; index < pixels.Length; index++)
            {
                Color32 pixel = pixels[index];
                byte luminance = pixel.r;

                if (pixel.g > luminance)
                    luminance = pixel.g;

                if (pixel.b > luminance)
                    luminance = pixel.b;

                int boostedAlpha = Mathf.Clamp(Mathf.RoundToInt(luminance * ALPHA_BOOST), 0, 255);
                pixel.a = (byte)boostedAlpha;
                pixels[index] = pixel;
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, false);
        }

        private static void ConfigureAsAbilitySprite(string assetPath)
        {
            TextureImporter textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;

            if (textureImporter == null)
            {
                Debug.LogError($"[ParticleVfxIconBaker] TextureImporter missing for {assetPath}");
                return;
            }

            textureImporter.textureType = TextureImporterType.Sprite;
            textureImporter.spriteImportMode = SpriteImportMode.Single;
            textureImporter.alphaIsTransparency = true;
            textureImporter.mipmapEnabled = false;
            textureImporter.npotScale = TextureImporterNPOTScale.None;
            textureImporter.wrapMode = TextureWrapMode.Clamp;
            textureImporter.filterMode = FilterMode.Bilinear;
            textureImporter.sRGBTexture = true;
            textureImporter.spritePixelsPerUnit = 100f;
            textureImporter.maxTextureSize = 512;

            TextureImporterPlatformSettings defaultSettings = textureImporter.GetDefaultPlatformTextureSettings();
            defaultSettings.maxTextureSize = 512;
            defaultSettings.format = TextureImporterFormat.Automatic;
            defaultSettings.textureCompression = TextureImporterCompression.Compressed;
            textureImporter.SetPlatformTextureSettings(defaultSettings);

            textureImporter.SaveAndReimport();
        }

        private readonly struct BakeJob
        {
            public BakeJob(string prefabPath, string fileName, float simulateSeconds, Vector3 cameraEuler)
            {
                PrefabPath = prefabPath;
                FileName = fileName;
                SimulateSeconds = simulateSeconds;
                CameraEuler = cameraEuler;
            }

            public string PrefabPath { get; }

            public string FileName { get; }

            public float SimulateSeconds { get; }

            public Vector3 CameraEuler { get; }
        }
    }
}
