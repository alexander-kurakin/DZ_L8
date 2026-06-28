using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Sectors
{
    [CreateAssetMenu(menuName = "Configs/Gameplay/Sectors/NewSectorGridConfig", fileName = "SectorGridConfig")]
    public class SectorGridConfig : ScriptableObject
    {
        [field: SerializeField, Min(0f)] public float InnerBeltMaxRadius { get; private set; } = 25.2f;
        [field: SerializeField, Min(0f)] public float MiddleBeltMaxRadius { get; private set; } = 45f;
        [field: SerializeField, Min(0f)] public float OuterBeltMaxRadius { get; private set; } = 64.8f;
        [field: SerializeField, Min(0f)] public float SpawnBeltMaxRadius { get; private set; } = 90f;

        [field: SerializeField, Min(0f)] public float InnerBeltAnchorRadius { get; private set; } = 12.6f;
        [field: SerializeField, Min(0f)] public float MiddleBeltAnchorRadius { get; private set; } = 35.1f;
        [field: SerializeField, Min(0f)] public float OuterBeltAnchorRadius { get; private set; } = 54.9f;
        [field: SerializeField, Min(0f)] public float SpawnBeltAnchorRadius { get; private set; } = 77.4f;
    }
}
