using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Assets._Project.Develop.Editor
{
    public static class SubEmitterStopActionFixer
    {
        private const string DEFAULT_VFX_ROOT = "Assets/_Project/Art/VFX";

        [MenuItem("Tools/VFX/Fix Sub-Emitter Stop Actions (Project VFX)")]
        public static void FixProjectVfxPrefabs()
        {
            FixPrefabsAtPaths(CollectPrefabPaths(DEFAULT_VFX_ROOT));
        }

        [MenuItem("Tools/VFX/Fix Sub-Emitter Stop Actions (Selection)")]
        public static void FixSelectedPrefabs()
        {
            List<string> prefabPaths = new List<string>();

            foreach (Object selectedObject in Selection.objects)
            {
                string assetPath = AssetDatabase.GetAssetPath(selectedObject);

                if (string.IsNullOrEmpty(assetPath))
                    continue;

                if (AssetDatabase.IsValidFolder(assetPath))
                {
                    prefabPaths.AddRange(CollectPrefabPaths(assetPath));
                    continue;
                }

                if (assetPath.EndsWith(".prefab"))
                    prefabPaths.Add(assetPath);
            }

            if (prefabPaths.Count == 0)
            {
                Debug.LogWarning("Sub-emitter stop action fixer: select one or more prefabs or a folder.");
                return;
            }

            FixPrefabsAtPaths(prefabPaths);
        }

        private static List<string> CollectPrefabPaths(string folderPath)
        {
            List<string> prefabPaths = new List<string>();
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });

            foreach (string guid in guids)
                prefabPaths.Add(AssetDatabase.GUIDToAssetPath(guid));

            return prefabPaths;
        }

        private static void FixPrefabsAtPaths(List<string> prefabPaths)
        {
            HashSet<string> uniquePaths = new HashSet<string>(prefabPaths);
            int changedPrefabCount = 0;
            int fixedSystemCount = 0;

            foreach (string prefabPath in uniquePaths)
            {
                int fixedInPrefab = FixPrefab(prefabPath);

                if (fixedInPrefab <= 0)
                    continue;

                changedPrefabCount++;
                fixedSystemCount += fixedInPrefab;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                $"Sub-emitter stop action fixer: updated {fixedSystemCount} particle system(s) "
                + $"in {changedPrefabCount} prefab(s).");
        }

        private static int FixPrefab(string prefabPath)
        {
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);

            if (prefabRoot == null)
                return 0;

            try
            {
                int fixedCount = FixSubEmitterStopActions(prefabRoot);

                if (fixedCount <= 0)
                    return 0;

                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);

                Debug.Log(
                    $"Sub-emitter stop action fixer: {Path.GetFileName(prefabPath)} — "
                    + $"set Stop Action = None on {fixedCount} sub-emitter(s).");

                return fixedCount;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        private static int FixSubEmitterStopActions(GameObject prefabRoot)
        {
            ParticleSystem[] particleSystems = prefabRoot.GetComponentsInChildren<ParticleSystem>(true);
            HashSet<ParticleSystem> subEmitterSystems = CollectSubEmitterSystems(particleSystems);
            int fixedCount = 0;

            foreach (ParticleSystem subEmitterSystem in subEmitterSystems)
            {
                ParticleSystem.MainModule mainModule = subEmitterSystem.main;

                if (mainModule.stopAction == ParticleSystemStopAction.None)
                    continue;

                mainModule.stopAction = ParticleSystemStopAction.None;
                fixedCount++;
            }

            return fixedCount;
        }

        private static HashSet<ParticleSystem> CollectSubEmitterSystems(ParticleSystem[] particleSystems)
        {
            HashSet<ParticleSystem> subEmitterSystems = new HashSet<ParticleSystem>();

            foreach (ParticleSystem particleSystem in particleSystems)
            {
                int subEmitterCount = particleSystem.subEmitters.subEmittersCount;

                for (int subEmitterIndex = 0; subEmitterIndex < subEmitterCount; subEmitterIndex++)
                {
                    ParticleSystem subEmitterSystem =
                        particleSystem.subEmitters.GetSubEmitterSystem(subEmitterIndex);

                    if (subEmitterSystem == null)
                        continue;

                    subEmitterSystems.Add(subEmitterSystem);
                }
            }

            return subEmitterSystems;
        }
    }
}
