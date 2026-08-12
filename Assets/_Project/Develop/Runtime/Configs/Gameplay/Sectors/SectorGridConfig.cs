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

        [field: SerializeField, Min(0f)] public float InnerBeltAnchorRadius { get; private set; } = 20f;
        [field: SerializeField, Min(0f)] public float MiddleBeltAnchorRadius { get; private set; } = 35.1f;
        [field: SerializeField, Min(0f)] public float OuterBeltAnchorRadius { get; private set; } = 54.9f;
        [field: SerializeField, Min(0f)] public float SpawnBeltAnchorRadius { get; private set; } = 77.4f;

        [field: SerializeField, Range(0f, 0.45f)] public float SpawnWedgeAngleMarginFraction { get; private set; } = 0.15f;
        [field: SerializeField, Range(0f, 0.45f)] public float SpawnWedgeRadiusMarginFraction { get; private set; } = 0.12f;
        [field: SerializeField, Min(0f)] public float SectorSurfaceGroundYOffset { get; private set; } = 0.25f;
        [field: SerializeField, Min(0f)] public float ClickArenaRadiusMargin { get; private set; } = 2f;
    }
}
