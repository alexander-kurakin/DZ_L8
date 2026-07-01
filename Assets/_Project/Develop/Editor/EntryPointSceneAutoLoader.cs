using Assets._Project.Develop.Runtime.Gameplay.Infrastructure;
using UnityEditor.SceneManagement;
using UnityEditor;

namespace Assets._Project.Develop.Editor
{
    [InitializeOnLoad]
    public static class EntryPointSceneAutoLoader
    {
        private const string PlayFromBootstrapMenuPath = "PlayFromBootstrap/Enabled";
        private const string SimulateFourWaveCompletionMenuPath = "PlayFromBootstrap/Simulate 4 wave completion";
        private const string PlayFromBootstrapKey = "PlayFromBootstrapKey";

        static EntryPointSceneAutoLoader()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                if (EditorPrefs.GetBool(PlayFromBootstrapKey) == false)
                {
                    EditorSceneManager.playModeStartScene = null;
                    return;
                }
            
                if (EditorBuildSettings.scenes.Length == 0)
                    return;

                EditorSceneManager.playModeStartScene = AssetDatabase
                    .LoadAssetAtPath<SceneAsset>(EditorBuildSettings.scenes[0].path);
            }
        }

        [MenuItem(PlayFromBootstrapMenuPath)]
        private static void TogglePlayFromBootstrap()
        {
            bool result = EditorPrefs.GetBool(PlayFromBootstrapKey);
            EditorPrefs.SetBool(PlayFromBootstrapKey, !result);
        }
        
        [MenuItem(PlayFromBootstrapMenuPath, true)]
        private static bool TogglePlayFromBootstrapValidate()
        {
            Menu.SetChecked(PlayFromBootstrapMenuPath, EditorPrefs.GetBool(PlayFromBootstrapKey));
            return true;
        }

        [MenuItem(SimulateFourWaveCompletionMenuPath)]
        private static void ToggleSimulateFourWaveCompletion()
        {
            bool result = EditorPrefs.GetBool(GameplayEditorDebugPrefs.SimulateFourWaveCompletionKey);
            EditorPrefs.SetBool(GameplayEditorDebugPrefs.SimulateFourWaveCompletionKey, result == false);
        }

        [MenuItem(SimulateFourWaveCompletionMenuPath, true)]
        private static bool ToggleSimulateFourWaveCompletionValidate()
        {
            Menu.SetChecked(
                SimulateFourWaveCompletionMenuPath,
                EditorPrefs.GetBool(GameplayEditorDebugPrefs.SimulateFourWaveCompletionKey));
            return true;
        }
    }
}