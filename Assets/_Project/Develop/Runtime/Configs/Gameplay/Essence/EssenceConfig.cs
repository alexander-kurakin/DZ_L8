using Assets._Project.Develop.Runtime.Configs.Gameplay.Stages;
using DamageNumbersPro;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Essence
{
    [CreateAssetMenu(menuName = "Configs/Gameplay/Essence/NewEssenceConfig", fileName = "EssenceConfig")]
    public class EssenceConfig : ScriptableObject
    {
        [field: SerializeField, Min(0)] public int StartEssencePerRun { get; private set; } = 0;
        [field: SerializeField, Min(0f)] public float HoverUnlockDelay { get; private set; } = 0.5f;
        [field: SerializeField, Min(0f)] public float VacuumMoveSpeed { get; private set; } = 30f;
        [field: SerializeField, Min(0f)] public float TowerCollectRadius { get; private set; } = 4f;
        [field: SerializeField, Min(0f)] public float TowerCollectHeightOffset { get; private set; } = 3.5f;
        [field: SerializeField, Range(0f, 1f)] public float TowerEatFraction { get; private set; } = 1f;
        [field: SerializeField, Range(0f, 1f)] public float PlantSellRefundFraction { get; private set; } = 0.5f;
        [field: SerializeField] public DamageNumber PlantSellRefundNumberPrefab { get; private set; }
        [field: SerializeField, Range(0f, 1f)] public float BailoutTowerHealthFraction { get; private set; } = 0.2f;
        [field: SerializeField, Min(0)] public int BailoutEssenceAmount { get; private set; } = 100;
        [field: SerializeField] public GameObject PickupGlowPrefab { get; private set; }
        [field: SerializeField] public GameObject PickupDropPrefab { get; private set; }
        [field: SerializeField] public GameObject PickupVacuumTrailPrefab { get; private set; }
        [field: SerializeField] public GameObject TowerCollectPrefab { get; private set; }
        [field: SerializeField, Min(0f)] public float TowerCollectVfxScale { get; private set; } = 6f;
        [field: SerializeField, Min(0f)] public float PickupDropVfxScale { get; private set; } = 0.2f;
        [field: SerializeField, Min(0f)] public float PickupGlowGroundScale { get; private set; } = 2.88f;
        [field: SerializeField, Min(0f)] public float PickupHoverColliderRadius { get; private set; } = 2.5f;
        [field: SerializeField, Min(0f)] public float PickupHoverColliderCenterY { get; private set; } = 9f;
        [field: SerializeField, Min(0f)] public float PickupFloorOffset { get; private set; } = 1.6f;
        [field: SerializeField, Min(0f)] public float PickupVacuumTrailScale { get; private set; } = 2f;
        [field: SerializeField, Min(0f)] public float PickupHoverReadyScaleFactor { get; private set; } = 1.35f;
        [field: SerializeField, Min(0f)] public float PickupHoverReadyGrowDurationSeconds { get; private set; } = 0.5f;
        [field: SerializeField, Min(0f)] public float PickupVacuumPulseScaleFactor { get; private set; } = 1.6f;
        [field: SerializeField, Min(0f)] public float PickupVacuumPulseUpDurationSeconds { get; private set; } = 0.22f;
        [field: SerializeField, Min(0f)] public float PickupVacuumSettleDurationSeconds { get; private set; } = 0.28f;

        [SerializeField] private List<EnemyEssenceDropEntry> _enemyDrops = new();

        public int GetDropAmountFor(WaveEnemyPreviewType previewType)
        {
            foreach (EnemyEssenceDropEntry entry in _enemyDrops)
            {
                if (entry.PreviewType == previewType)
                    return entry.DropAmount;
            }

            throw new InvalidOperationException($"Essence drop for {previewType} is not configured in EssenceConfig.");
        }

        [Serializable]
        private class EnemyEssenceDropEntry
        {
            [field: SerializeField] public WaveEnemyPreviewType PreviewType { get; private set; }
            [field: SerializeField, Min(0)] public int DropAmount { get; private set; }
        }
    }
}
