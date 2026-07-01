using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Spellcore
{
    [CreateAssetMenu(
        menuName = "Configs/Gameplay/Spellcore/NewSpellcoreCombatConfig",
        fileName = "SpellcoreCombatConfig")]
    public class SpellcoreCombatConfig : ScriptableObject
    {
        [SerializeField, Range(0.1f, 1f)] private float _enemyMoveSpeedScale = 0.7f;

        [SerializeField, Min(0)] private int _waveOnePrepFreeMines = 1;

        [SerializeField, Min(1)] private int _minePulsesPerSectorCrossing = 3;

        [SerializeField, Min(0.1f)] private float _referenceCatMoveSpeed = 4f;

        [SerializeField, Range(0.5f, 1f)] private float _mineLastPulseProgressThreshold = 0.92f;

        [SerializeField, Range(0.5f, 1f)] private float _mineLastPulseTimeFraction = 0.92f;

        [SerializeField, Min(1f)] private float _mineDamagePerPulse = 115f;

        [SerializeField, Range(0.01f, 1f)] private float _tankFirstMinePulseDamageMultiplier = 0.15f;

        [SerializeField, Range(0.1f, 0.9f)] private float _toxicSlowMoveSpeedFraction = 0.5f;

        [SerializeField, Min(1)] private int _buildingBuffMaxActiveCount = 2;

        [SerializeField, Min(1f)] private float _buildingBuffDamageMultiplier = 1.5f;

        [SerializeField, Min(1f)] private float _buildingBuffDurationSeconds = 60f;

        [SerializeField, Min(0)] private int _buildingBuffEssenceCost = 10;

        [SerializeField, Min(1)] private int _brotherRepairHitsPerMovementPhase = 1;

        public float EnemyMoveSpeedScale => _enemyMoveSpeedScale;

        public int WaveOnePrepFreeMines => _waveOnePrepFreeMines;

        public int MinePulsesPerSectorCrossing => _minePulsesPerSectorCrossing;

        public float ReferenceCatMoveSpeed => _referenceCatMoveSpeed;

        public float MineLastPulseProgressThreshold => _mineLastPulseProgressThreshold;

        public float MineLastPulseTimeFraction => _mineLastPulseTimeFraction;

        public float MineDamagePerPulse => _mineDamagePerPulse;

        public float TankFirstMinePulseDamageMultiplier => _tankFirstMinePulseDamageMultiplier;

        public float ToxicSlowMoveSpeedFraction => _toxicSlowMoveSpeedFraction;

        public int BuildingBuffMaxActiveCount => _buildingBuffMaxActiveCount;

        public float BuildingBuffDamageMultiplier => _buildingBuffDamageMultiplier;

        public float BuildingBuffDurationSeconds => _buildingBuffDurationSeconds;

        public int BuildingBuffEssenceCost => _buildingBuffEssenceCost;

        public int BrotherRepairHitsPerMovementPhase => _brotherRepairHitsPerMovementPhase;
    }
}
