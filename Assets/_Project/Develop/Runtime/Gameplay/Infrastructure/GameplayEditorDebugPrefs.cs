namespace Assets._Project.Develop.Runtime.Gameplay.Infrastructure
{
    public static class GameplayEditorDebugPrefs
    {
        public const string SimulateFourWaveCompletionKey = "SimulateFourWaveCompletionKey";

        public static bool IsSimulateFourWaveCompletionEnabled
        {
            get
            {
#if UNITY_EDITOR
                return UnityEditor.EditorPrefs.GetBool(SimulateFourWaveCompletionKey);
#else
                return false;
#endif
            }
        }
    }
}
