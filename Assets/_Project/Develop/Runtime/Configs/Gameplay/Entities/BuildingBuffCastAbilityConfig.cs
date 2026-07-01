using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Entities
{
    [CreateAssetMenu(menuName = "Configs/Gameplay/Entities/NewBuildingBuffCastAbilityConfig",
        fileName = "BuildingBuffCastAbilityConfig")]
    public class BuildingBuffCastAbilityConfig : EntityConfig
    {
        [field: SerializeField, Min(0)] public float CooldownSeconds { get; private set; } = 5f;
        [field: SerializeField] public GameObject FrostProjectilePrefab { get; private set; }
        [field: SerializeField, Min(0)] public float FrostProjectileSpeed { get; private set; } = 45f;
        [field: SerializeField, Min(0)] public float FrostProjectileScale { get; private set; } = 1.5f;
        [field: SerializeField] public GameObject FrostTargetOrbsPrefab { get; private set; }
        [field: SerializeField, Min(0)] public float FrostTargetOrbsScale { get; private set; } = 1f;
        [field: SerializeField, Min(0)] public float FrostTargetOrbsLingerAfterImpactSeconds { get; private set; } = 0.18f;
        [field: SerializeField, Min(0)] public float ImpactGroundYOffset { get; private set; } = 0.25f;
        [field: SerializeField, Min(0)] public float CastWindupSeconds { get; private set; } = 0.28f;
        [field: SerializeField, Min(0)] public float ProjectileSpawnHeight { get; private set; } = 2.6f;
        [field: SerializeField] public float ProjectileYawOffsetDegrees { get; private set; } = -90f;
        [field: SerializeField, Min(0)] public float PreviewIndicatorDiameter { get; private set; } = 10f;
        [field: SerializeField, Min(0)] public float PreviewReferenceDiameter { get; private set; } = 10f;
        [field: SerializeField, Min(0)] public float PreviewPaddingMultiplier { get; private set; } = 2f;
        [field: SerializeField, Min(0)] public float CastVfxScaleMultiplier { get; private set; } = 0.85f;
        [field: SerializeField] public GameObject IceFloorVfxPrefab { get; private set; }
        [field: SerializeField, Min(0)] public float BuildingBuffFollowupDelaySeconds { get; private set; } = 0.2f;
        [field: SerializeField, Min(0)] public float CooldownFillYOffset { get; private set; } = 0.01f;
        [field: SerializeField] public Color CooldownFillColor { get; private set; } = new Color(1f, 0.15f, 0.15f, 0.6f);
    }
}
