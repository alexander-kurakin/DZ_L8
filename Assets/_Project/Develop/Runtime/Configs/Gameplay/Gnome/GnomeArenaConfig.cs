using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Gnome
{
    [CreateAssetMenu(menuName = "Configs/Gameplay/Gnome/NewGnomeArenaConfig", fileName = "GnomeArenaConfig")]
    public class GnomeArenaConfig : ScriptableObject
    {
        [field: SerializeField, Min(1)] public int TotalGnomesInRun { get; private set; } = 10;
        [field: SerializeField, Min(1)] public int MaxActiveGnomes { get; private set; } = 4;
        [field: SerializeField, Min(1)] public int TargetGnomesToKill { get; private set; } = 10;
        [field: SerializeField, Min(0f)] public float RespawnDelaySeconds { get; private set; } = 1.5f;
        [field: SerializeField, Min(0f)] public float GazeCheckIntervalSeconds { get; private set; } = 0.15f;
        [field: SerializeField, Min(0f)] public float GazeTriggerRadius { get; private set; } = 1.25f;
        [field: SerializeField] public LayerMask GazeCoverLayerMask { get; private set; } = ~0;
    }
}
