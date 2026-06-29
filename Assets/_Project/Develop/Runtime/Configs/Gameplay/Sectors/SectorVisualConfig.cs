using System;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Sectors
{
    [CreateAssetMenu(menuName = "Configs/Gameplay/Sectors/NewSectorVisualConfig", fileName = "SectorVisualConfig")]
    public class SectorVisualConfig : ScriptableObject
    {
        [SerializeField] private SectorFillVisualData _unlockedFill = new SectorFillVisualData(new Color(0.2f, 0.85f, 0.35f, 1f), 0.35f);
        [SerializeField] private SectorFillVisualData _lockedFill = new SectorFillVisualData(new Color(0.55f, 0.55f, 0.55f, 1f), 0.35f);
        [SerializeField] private Color _outlineColor = new Color(0.95f, 1f, 1f, 1f);
        [SerializeField, Min(0.01f)] private float _outlineWidth = 0.35f;
        [SerializeField] private Color _spawnPreviewOutlineColor = new Color(1f, 0.92f, 0.35f, 1f);
        [SerializeField, Min(0.01f)] private float _spawnPreviewOutlineWidth = 0.55f;
        [SerializeField] private Sprite _spawnPathArrowSprite;
        [SerializeField, Min(0f)] private float _unlockRevealDurationSeconds = 0.65f;
        [SerializeField, Min(0f)] private float _beltRevealStepSeconds = 0.85f;
        [SerializeField, Min(0)] private int _spawnPreviewRevealStepOrder = 3;

        public SectorFillVisualData UnlockedFill => _unlockedFill;

        public SectorFillVisualData LockedFill => _lockedFill;

        public Color OutlineColor => _outlineColor;

        public float OutlineWidth => _outlineWidth;

        public Color SpawnPreviewOutlineColor => _spawnPreviewOutlineColor;

        public float SpawnPreviewOutlineWidth => _spawnPreviewOutlineWidth;

        public Sprite SpawnPathArrowSprite => _spawnPathArrowSprite;

        public float UnlockRevealDurationSeconds => _unlockRevealDurationSeconds;

        public float BeltRevealStepSeconds => _beltRevealStepSeconds;

        public int SpawnPreviewRevealStepOrder => _spawnPreviewRevealStepOrder;

        public float GetRevealStepDelaySeconds(int stepOrder)
        {
            return stepOrder * _beltRevealStepSeconds;
        }
    }

    [Serializable]
    public struct SectorFillVisualData
    {
        public SectorFillVisualData(Color color, float alpha)
        {
            Color = color;
            Alpha = alpha;
        }

        [field: SerializeField] public Color Color { get; private set; }

        [field: SerializeField, Range(0f, 1f)] public float Alpha { get; private set; }
    }
}
