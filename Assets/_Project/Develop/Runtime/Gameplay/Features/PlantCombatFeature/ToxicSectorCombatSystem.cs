using System.Collections.Generic;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Stages;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Gameplay.Features.JuiceFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.PlantBuildingBuff;
using Assets._Project.Develop.Runtime.Gameplay.Features.PlantCombatFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.PlantCombatFeature
{
    public class ToxicSectorCombatSystem : IInitializableSystem, IUpdatableSystem
    {
        private readonly SectorEnemyQueryService _sectorEnemyQueryService;
        private readonly PlantDamageApplicationService _plantDamageApplicationService;
        private readonly PlantDamageCounterService _plantDamageCounterService;
        private readonly GameplayJuiceService _gameplayJuiceService;
        private readonly PlantBuildingBuffJuiceService _plantBuildingBuffJuiceService;

        private readonly List<Entity> _enemiesInSector = new();
        private readonly HashSet<Entity> _slowedEnemies = new();
        private readonly Dictionary<Entity, float> _baseMoveSpeedByEnemy = new();
        private readonly Dictionary<Entity, GameObject> _slowAuraByEnemy = new();

        private Entity _toxicEntity;
        private PlantSector _plantSector;
        private ReactiveVariable<float> _damagePerTick;
        private ReactiveVariable<float> _dotTickInterval;
        private ReactiveVariable<float> _dotTimer;
        private float _slowMoveSpeedFraction;
        private GameObject _slowAuraPrefab;
        private float _slowAuraBaseScale;
        private Vector3 _slowAuraLocalPositionOffset;
        private Vector3 _slowAuraLocalScaleMultiplier;

        public ToxicSectorCombatSystem(
            SectorEnemyQueryService sectorEnemyQueryService,
            PlantDamageApplicationService plantDamageApplicationService,
            PlantDamageCounterService plantDamageCounterService,
            GameplayJuiceService gameplayJuiceService,
            PlantBuildingBuffJuiceService plantBuildingBuffJuiceService,
            float slowMoveSpeedFraction,
            GameObject slowAuraPrefab,
            float slowAuraBaseScale,
            Vector3 slowAuraLocalPositionOffset,
            Vector3 slowAuraLocalScaleMultiplier)
        {
            _sectorEnemyQueryService = sectorEnemyQueryService;
            _plantDamageApplicationService = plantDamageApplicationService;
            _plantDamageCounterService = plantDamageCounterService;
            _gameplayJuiceService = gameplayJuiceService;
            _plantBuildingBuffJuiceService = plantBuildingBuffJuiceService;
            _slowMoveSpeedFraction = slowMoveSpeedFraction;
            _slowAuraPrefab = slowAuraPrefab;
            _slowAuraBaseScale = slowAuraBaseScale;
            _slowAuraLocalPositionOffset = slowAuraLocalPositionOffset;
            _slowAuraLocalScaleMultiplier = slowAuraLocalScaleMultiplier;
        }

        public void OnInit(Entity entity)
        {
            _toxicEntity = entity;
            _plantSector = entity.PlantSectorC;
            _damagePerTick = entity.DamagePerTick;
            _dotTickInterval = entity.DamageInterval;
            _dotTimer = entity.DamageTimer;
        }

        public void OnUpdate(float deltaTime)
        {
            _dotTimer.Value -= deltaTime;

            if (_dotTimer.Value > 0f)
                return;

            _sectorEnemyQueryService.CollectEnemiesInSector(_plantSector.Value, _enemiesInSector);
            HashSet<Entity> enemiesInSectorThisTick = new();

            for (int index = 0; index < _enemiesInSector.Count; index++)
            {
                Entity enemy = _enemiesInSector[index];
                enemiesInSectorThisTick.Add(enemy);

                bool damageApplied = _plantDamageApplicationService.TryApplyDamage(
                    _toxicEntity,
                    enemy,
                    _damagePerTick.Value,
                    PlantDamageSource.Toxic);

                if (damageApplied)
                {
                    _gameplayJuiceService.PlayToxicTick(enemy);

                    if (_plantBuildingBuffJuiceService.IsBuffed(_toxicEntity))
                        _plantBuildingBuffJuiceService.PlayBuffedToxicTick();
                }

                TryApplySlow(enemy);
            }

            RestoreSlowForEnemiesLeftSector(enemiesInSectorThisTick);
            _dotTimer.Value = _dotTickInterval.Value;
        }

        private void TryApplySlow(Entity enemy)
        {
            if (enemy.HasComponent<FlyingEnemy>())
                return;

            if (enemy.TryGetEnemyWavePreviewType(out WaveEnemyPreviewType previewType) == false)
                return;

            if (_plantDamageCounterService.ShouldApplyToxicSlow(previewType) == false)
                return;

            if (enemy.TryGetMoveSpeed(out ReactiveVariable<float> moveSpeed) == false)
                return;

            if (_baseMoveSpeedByEnemy.ContainsKey(enemy) == false)
                _baseMoveSpeedByEnemy[enemy] = moveSpeed.Value;

            float baseMoveSpeed = _baseMoveSpeedByEnemy[enemy];
            moveSpeed.Value = baseMoveSpeed * (1f - _slowMoveSpeedFraction);
            _slowedEnemies.Add(enemy);
            EnsureSlowAura(enemy);
        }

        private void RestoreSlowForEnemiesLeftSector(HashSet<Entity> enemiesInSectorThisTick)
        {
            List<Entity> enemiesToRestore = new();

            foreach (Entity slowedEnemy in _slowedEnemies)
            {
                if (enemiesInSectorThisTick.Contains(slowedEnemy))
                    continue;

                enemiesToRestore.Add(slowedEnemy);
            }

            for (int index = 0; index < enemiesToRestore.Count; index++)
            {
                Entity enemy = enemiesToRestore[index];
                RestoreMoveSpeed(enemy);
                _slowedEnemies.Remove(enemy);
                _baseMoveSpeedByEnemy.Remove(enemy);
            }
        }

        private void RestoreMoveSpeed(Entity enemy)
        {
            if (_baseMoveSpeedByEnemy.TryGetValue(enemy, out float baseMoveSpeed) == false)
                return;

            if (enemy.TryGetMoveSpeed(out ReactiveVariable<float> moveSpeed) == false)
                return;

            moveSpeed.Value = baseMoveSpeed;
            RemoveSlowAura(enemy);
        }

        private void EnsureSlowAura(Entity enemy)
        {
            if (_slowAuraPrefab == null)
                return;

            if (_slowAuraByEnemy.ContainsKey(enemy))
                return;

            if (enemy.TryGetTransform(out Transform enemyTransform) == false)
                return;

            GameObject auraInstance = GameplayVfxUtility.SpawnAt(
                _slowAuraPrefab,
                enemyTransform.position,
                Quaternion.identity,
                enemyTransform,
                1f);

            ApplySlowAuraTransform(auraInstance.transform);
            _slowAuraByEnemy[enemy] = auraInstance;
        }

        private void ApplySlowAuraTransform(Transform auraTransform)
        {
            auraTransform.localPosition = _slowAuraLocalPositionOffset;
            auraTransform.localScale = new Vector3(
                _slowAuraBaseScale * _slowAuraLocalScaleMultiplier.x,
                _slowAuraBaseScale * _slowAuraLocalScaleMultiplier.y,
                _slowAuraBaseScale * _slowAuraLocalScaleMultiplier.z);
        }

        private void RemoveSlowAura(Entity enemy)
        {
            if (_slowAuraByEnemy.TryGetValue(enemy, out GameObject auraInstance) == false)
                return;

            if (auraInstance != null)
                Object.Destroy(auraInstance);

            _slowAuraByEnemy.Remove(enemy);
        }
    }
}
