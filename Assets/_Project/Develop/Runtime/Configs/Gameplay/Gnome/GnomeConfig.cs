using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Gnome
{
    [CreateAssetMenu(menuName = "Configs/Gameplay/Gnome/NewGnomeConfig", fileName = "GnomeConfig")]
    public class GnomeConfig : EntityConfig
    {
        [field: SerializeField, Min(1)] public int RequiredHits { get; private set; } = 2;
        [field: SerializeField, Min(0f)] public float HiddenDurationMinSeconds { get; private set; } = 1.2f;
        [field: SerializeField, Min(0f)] public float HiddenDurationMaxSeconds { get; private set; } = 2.4f;
        [field: SerializeField, Min(0f)] public float PeekDurationMinSeconds { get; private set; } = 1.8f;
        [field: SerializeField, Min(0f)] public float PeekDurationMaxSeconds { get; private set; } = 2.5f;
        [field: SerializeField, Min(0f)] public float DefaultPeekOffset { get; private set; } = 0.55f;
        [field: SerializeField, Min(0f)] public float DeathDissolveSeconds { get; private set; } = 0.45f;
    }
}
