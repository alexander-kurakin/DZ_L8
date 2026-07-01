using System.Collections.Generic;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Spellcore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Gameplay.Features.PlantBuildingBuff;
using Assets._Project.Develop.Runtime.Gameplay.Features.PlantCombatFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.JuiceFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Mines
{
    public class MineFactoryPulseDetonationSystem : IInitializableSystem, IUpdatableSystem
    {
        private struct EnemyMinePulseState
        {
            public int PulsesDelivered;
            public float TimeInSector;
            public float TimeAfterStopPulse;
            public float MaxInboundProgress;
        }

        private readonly SectorEnemyQueryService _sectorEnemyQueryService;
        private readonly PlantDamageApplicationService _plantDamageApplicationService;
        private readonly GameplayJuiceService _gameplayJuiceService;
        private readonly SectorRegistryService _sectorRegistryService;
        private readonly SpellcoreCombatConfig _spellcoreCombatConfig;
        private readonly PlantBuildingBuffJuiceService _plantBuildingBuffJuiceService;

        private readonly Dictionary<Entity, EnemyMinePulseState> _enemyPulseStates = new();
        private readonly List<Entity> _enemiesInSector = new();
        private readonly List<Entity> _staleEnemies = new();

        private Entity _mineEntity;
        private Transform _mineTransform;
        private ReactiveVariable<float> _mineDamage;
        private ReactiveEvent<Vector3> _dealAreaImpactDamageRequest;
        private PlantSector _plantSector;

        public MineFactoryPulseDetonationSystem(
            SectorEnemyQueryService sectorEnemyQueryService,
            PlantDamageApplicationService plantDamageApplicationService,
            GameplayJuiceService gameplayJuiceService,
            SectorRegistryService sectorRegistryService,
            SpellcoreCombatConfig spellcoreCombatConfig,
            PlantBuildingBuffJuiceService plantBuildingBuffJuiceService)
        {
            _sectorEnemyQueryService = sectorEnemyQueryService;
            _plantDamageApplicationService = plantDamageApplicationService;
            _gameplayJuiceService = gameplayJuiceService;
            _sectorRegistryService = sectorRegistryService;
            _spellcoreCombatConfig = spellcoreCombatConfig;
            _plantBuildingBuffJuiceService = plantBuildingBuffJuiceService;
        }

        public void OnInit(Entity entity)
        {
            _mineEntity = entity;
            _dealAreaImpactDamageRequest = entity.DealAreaImpactDamageRequest;
            _mineTransform = entity.Transform;
            _mineDamage = entity.AreaImpactDamage;
            _plantSector = entity.PlantSectorC;
        }

        public void OnUpdate(float deltaTime)
        {
            _sectorEnemyQueryService.CollectEnemiesInMineSector(
                _plantSector.Value,
                _sectorRegistryService.Center,
                _mineTransform.position,
                _sectorRegistryService.GridConfig,
                MineFactoryPulseBehaviorUtility.ShouldIncludeMineCellProximity(_plantSector.Value.Belt),
                _enemiesInSector);

            FlushRemainingPulsesOnExit();
            UpdateEnemyPulseStates(deltaTime);
        }

        private void FlushRemainingPulsesOnExit()
        {
            _staleEnemies.Clear();
            bool pulseTriggeredThisFrame = false;

            foreach (KeyValuePair<Entity, EnemyMinePulseState> entry in _enemyPulseStates)
            {
                Entity enemy = entry.Key;
                EnemyMinePulseState pulseState = entry.Value;
                int maxPulses = _spellcoreCombatConfig.MinePulsesPerSectorCrossing;
                bool enemyLeftSector = _enemiesInSector.Contains(enemy) == false;
                bool enemyIsDead = enemy.IsDead.Value;

                if (enemyLeftSector == false && enemyIsDead == false)
                    continue;

                while (pulseState.PulsesDelivered < maxPulses)
                {
                    if (TryDeliverPulse(enemy, ref pulseState) == false)
                        break;

                    pulseTriggeredThisFrame = true;
                }

                _staleEnemies.Add(enemy);
            }

            if (pulseTriggeredThisFrame)
                PlayMinePulseJuice();

            for (int index = 0; index < _staleEnemies.Count; index++)
                _enemyPulseStates.Remove(_staleEnemies[index]);
        }

        private void UpdateEnemyPulseStates(float deltaTime)
        {
            int maxPulses = _spellcoreCombatConfig.MinePulsesPerSectorCrossing;
            bool pulseTriggeredThisFrame = false;

            for (int index = 0; index < _enemiesInSector.Count; index++)
            {
                Entity enemy = _enemiesInSector[index];

                if (enemy.IsDead.Value)
                    continue;

                if (enemy.TryGetTransform(out Transform enemyTransform) == false)
                    continue;

                if (_enemyPulseStates.TryGetValue(enemy, out EnemyMinePulseState pulseState) == false)
                {
                    pulseState = new EnemyMinePulseState
                    {
                        PulsesDelivered = 0,
                        TimeInSector = 0f,
                        TimeAfterStopPulse = 0f,
                        MaxInboundProgress = 0f,
                    };
                }

                pulseState.TimeInSector += deltaTime;

                if (pulseState.PulsesDelivered >= 2)
                    pulseState.TimeAfterStopPulse += deltaTime;

                if (pulseState.PulsesDelivered >= maxPulses)
                {
                    _enemyPulseStates[enemy] = pulseState;
                    continue;
                }

                if (IsPulseReady(enemy, enemyTransform, ref pulseState) == false)
                {
                    _enemyPulseStates[enemy] = pulseState;
                    continue;
                }

                if (TryDeliverPulse(enemy, ref pulseState))
                    pulseTriggeredThisFrame = true;

                _enemyPulseStates[enemy] = pulseState;
            }

            if (pulseTriggeredThisFrame)
                PlayMinePulseJuice();
        }

        private bool IsPulseReady(Entity enemy, Transform enemyTransform, ref EnemyMinePulseState pulseState)
        {
            SectorBelt mineBelt = _plantSector.Value.Belt;
            int pulsesPerCrossing = _spellcoreCombatConfig.MinePulsesPerSectorCrossing;

            if (MineFactoryPulseBehaviorUtility.UsesStopBeltMinePattern(enemy, mineBelt))
                return IsStopBeltPulseReady(enemy, ref pulseState, pulsesPerCrossing);

            return IsFullCrossingPulseReady(enemyTransform, ref pulseState, mineBelt, pulsesPerCrossing);
        }

        private bool IsStopBeltPulseReady(Entity enemy, ref EnemyMinePulseState pulseState, int pulsesPerCrossing)
        {
            if (pulseState.PulsesDelivered == 0)
                return true;

            if (pulseState.PulsesDelivered == 1)
                return HasReachedStopPoint(enemy);

            float enemyMoveSpeed = ResolveEnemyMoveSpeed(enemy);
            float sectorCrossSeconds = MineFactoryPulseTimingUtility.ResolveSectorCrossSeconds(
                _plantSector.Value.Belt,
                _sectorRegistryService.GridConfig,
                enemyMoveSpeed);

            float timeThreshold = MineFactoryPulseTimingUtility.ResolveStopBeltThirdPulseDelay(
                sectorCrossSeconds,
                pulsesPerCrossing,
                _spellcoreCombatConfig.MineLastPulseTimeFraction);

            return pulseState.TimeAfterStopPulse >= timeThreshold;
        }

        private bool IsFullCrossingPulseReady(
            Transform enemyTransform,
            ref EnemyMinePulseState pulseState,
            SectorBelt mineBelt,
            int pulsesPerCrossing)
        {
            float inboundProgress = WorldToSector.ResolveInboundBeltProgress(
                enemyTransform.position,
                _sectorRegistryService.Center,
                mineBelt,
                _sectorRegistryService.GridConfig);

            if (inboundProgress > pulseState.MaxInboundProgress)
                pulseState.MaxInboundProgress = inboundProgress;

            float progressThreshold = MineFactoryPulseTimingUtility.ResolvePulseProgressThreshold(
                pulseState.PulsesDelivered,
                pulsesPerCrossing,
                _spellcoreCombatConfig.MineLastPulseProgressThreshold);

            if (pulseState.MaxInboundProgress >= progressThreshold)
                return true;

            return false;
        }

        private static bool HasReachedStopPoint(Entity enemy)
        {
            if (enemy.TryGetDistanceToTargetReached(out ReactiveVariable<bool> distanceReached) == false)
                return false;

            return distanceReached.Value;
        }

        private bool TryDeliverPulse(Entity enemy, ref EnemyMinePulseState pulseState)
        {
            bool damageApplied = _plantDamageApplicationService.TryApplyDamage(
                _mineEntity,
                enemy,
                _mineDamage.Value,
                PlantDamageSource.Mine);

            if (damageApplied == false)
                return false;

            pulseState.PulsesDelivered++;
            return true;
        }

        private void PlayMinePulseJuice()
        {
            _gameplayJuiceService.PlayScreenShakeSmall();
            Vector3 pulseWorldPoint = _mineTransform.position;
            _dealAreaImpactDamageRequest?.Invoke(pulseWorldPoint);

            if (_plantBuildingBuffJuiceService.IsBuffed(_mineEntity))
                _plantBuildingBuffJuiceService.PlayBuffedMinePulseFollowup(pulseWorldPoint);
        }

        private float ResolveEnemyMoveSpeed(Entity enemy)
        {
            if (enemy.TryGetMoveSpeed(out ReactiveVariable<float> moveSpeed))
                return moveSpeed.Value;

            return MineFactoryPulseTimingUtility.ResolveFallbackEnemyMoveSpeed(_spellcoreCombatConfig);
        }
    }
}
