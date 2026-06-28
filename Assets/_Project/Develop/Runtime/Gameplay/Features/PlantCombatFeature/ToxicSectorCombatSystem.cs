using System.Collections.Generic;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Stages;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Gameplay.Features.PlantCombatFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature;
using Assets._Project.Develop.Runtime.Utilities.Reactive;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.PlantCombatFeature
{
    public class ToxicSectorCombatSystem : IInitializableSystem, IUpdatableSystem
    {
        private readonly SectorEnemyQueryService _sectorEnemyQueryService;
        private readonly PlantDamageApplicationService _plantDamageApplicationService;
        private readonly PlantDamageCounterService _plantDamageCounterService;

        private readonly List<Entity> _enemiesInSector = new();
        private readonly HashSet<Entity> _slowedEnemies = new();
        private readonly Dictionary<Entity, float> _baseMoveSpeedByEnemy = new();

        private Entity _toxicEntity;
        private PlantSector _plantSector;
        private ReactiveVariable<float> _damagePerTick;
        private ReactiveVariable<float> _dotTickInterval;
        private ReactiveVariable<float> _dotTimer;
        private float _slowMoveSpeedFraction;

        public ToxicSectorCombatSystem(
            SectorEnemyQueryService sectorEnemyQueryService,
            PlantDamageApplicationService plantDamageApplicationService,
            PlantDamageCounterService plantDamageCounterService,
            float slowMoveSpeedFraction)
        {
            _sectorEnemyQueryService = sectorEnemyQueryService;
            _plantDamageApplicationService = plantDamageApplicationService;
            _plantDamageCounterService = plantDamageCounterService;
            _slowMoveSpeedFraction = slowMoveSpeedFraction;
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

                _plantDamageApplicationService.TryApplyDamage(
                    _toxicEntity,
                    enemy,
                    _damagePerTick.Value,
                    PlantDamageSource.Toxic);

                TryApplySlow(enemy);
            }

            RestoreSlowForEnemiesLeftSector(enemiesInSectorThisTick);
            _dotTimer.Value = _dotTickInterval.Value;
        }

        private void TryApplySlow(Entity enemy)
        {
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
        }
    }
}
