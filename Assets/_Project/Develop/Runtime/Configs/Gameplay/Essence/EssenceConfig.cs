using Assets._Project.Develop.Runtime.Configs.Gameplay.Stages;
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
        [field: SerializeField, Range(0f, 1f)] public float TowerEatFraction { get; private set; } = 1f;
        [field: SerializeField, Range(0f, 1f)] public float PlantSellRefundFraction { get; private set; } = 0.35f;

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
