using System;
using System.Collections.Generic;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Sectors;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Spellcore;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Stages;
using Assets._Project.Develop.Runtime.Gameplay.Features.Ability;
using Assets._Project.Develop.Runtime.Gameplay.Infrastructure;
using Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature;
using Assets._Project.Develop.Runtime.Utilities.ConfigsManagment;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.SpellcoreProgressionFeature
{
    public class SpellcoreProgressionService : IDisposable
    {
        private readonly SpellcoreProgressionConfig _config;
        private readonly SectorRegistryService _sectorRegistryService;
        private readonly StageProviderService _stageProviderService;
        private readonly SurvivalFlowService _survivalFlowService;
        private readonly ConfigsProviderService _configsProviderService;
        private readonly SpawnPathPreviewService _spawnPathPreviewService;
        private readonly WaveSpawnPlanService _waveSpawnPlanService;
        private readonly PathUnlockSequenceService _pathUnlockSequenceService;
        private readonly SpellcoreCombatConfig _spellcoreCombatConfig;

        private int _completedWaves;
        private int _freeMinesRemaining;
        private bool _showSpawnPathPreview;
        private readonly List<int> _runUnlockedPathIndices = new();
        private readonly List<int> _pendingUnlockRevealPathIndices = new();
        private IDisposable _stageCompletedSubscription;

        public SpellcoreProgressionService(
            SpellcoreProgressionConfig config,
            SectorRegistryService sectorRegistryService,
            StageProviderService stageProviderService,
            SurvivalFlowService survivalFlowService,
            ConfigsProviderService configsProviderService,
            SpawnPathPreviewService spawnPathPreviewService,
            WaveSpawnPlanService waveSpawnPlanService,
            PathUnlockSequenceService pathUnlockSequenceService,
            SpellcoreCombatConfig spellcoreCombatConfig)
        {
            _config = config;
            _sectorRegistryService = sectorRegistryService;
            _stageProviderService = stageProviderService;
            _survivalFlowService = survivalFlowService;
            _configsProviderService = configsProviderService;
            _spawnPathPreviewService = spawnPathPreviewService;
            _waveSpawnPlanService = waveSpawnPlanService;
            _pathUnlockSequenceService = pathUnlockSequenceService;
            _spellcoreCombatConfig = spellcoreCombatConfig;
        }

        public event Action Changed;

        public int CompletedWaves => _completedWaves;

        public int UpcomingWaveNumber => _completedWaves + 1;

        public int FreeMinesRemaining => _freeMinesRemaining;

        public void InitializeForRun()
        {
            _completedWaves = 0;
            _freeMinesRemaining = _spellcoreCombatConfig.WaveOnePrepFreeMines;

            _runUnlockedPathIndices.Clear();
            _pendingUnlockRevealPathIndices.Clear();
            _stageCompletedSubscription = _stageProviderService.StageCompleted.Subscribe(OnWaveCompleted);

            ApplyPathsForWave(UpcomingWaveNumber);
            RefreshSectorVisuals();
            Changed?.Invoke();
        }

        public void ApplyDebugSimulatedCompletedWaves(int completedWaves)
        {
            _completedWaves = completedWaves;
            _freeMinesRemaining = _spellcoreCombatConfig.WaveOnePrepFreeMines;

            if (completedWaves >= 1)
                _freeMinesRemaining += _config.StarterFreeMines;

            _runUnlockedPathIndices.Clear();
            _pendingUnlockRevealPathIndices.Clear();
            _showSpawnPathPreview = true;

            ApplyPathsForWave(UpcomingWaveNumber);
            _pendingUnlockRevealPathIndices.Clear();
            RefreshSectorVisuals();
            Changed?.Invoke();
        }

        public void OnPreparationEntered()
        {
            _showSpawnPathPreview = true;
            ApplyPathsForWave(UpcomingWaveNumber);
            RefreshSectorVisuals();
            TryPlayPendingPathUnlockReveal();
            Changed?.Invoke();
        }

        public void OnCombatWaveEntered(int waveNumber)
        {
            _pathUnlockSequenceService.Cancel();
            _showSpawnPathPreview = false;
            RefreshSectorVisuals();
            Changed?.Invoke();
        }

        public bool HasAnyPlantAbilityUnlocked()
        {
            return IsAbilityUnlocked(AbilityType.PlantMine)
                   || IsAbilityUnlocked(AbilityType.PlantTurret)
                   || IsAbilityUnlocked(AbilityType.PlantToxicArea);
        }

        public bool IsAbilityUnlocked(AbilityType abilityType)
        {
            switch (abilityType)
            {
                case AbilityType.PlantMine:
                    return true;

                case AbilityType.PlantTurret:
                    return _completedWaves >= 2;

                case AbilityType.PlantToxicArea:
                    return _completedWaves >= 3;

                default:
                    return true;
            }
        }

        public bool TrySpendFreeMine()
        {
            if (_freeMinesRemaining <= 0)
                return false;

            _freeMinesRemaining--;
            Changed?.Invoke();
            return true;
        }

        public void Dispose()
        {
            _stageCompletedSubscription?.Dispose();
        }

        private void OnWaveCompleted()
        {
            _completedWaves++;

            if (_completedWaves == 1)
                _freeMinesRemaining += _config.StarterFreeMines;

            if (_completedWaves == _stageProviderService.StagesCount)
                _survivalFlowService.OnNormalCampaignCompleted();
            else if (_stageProviderService.IsSurvivalModeActive
                     && SurvivalWaveScalingService.IsSurvivalMilestoneCompletedWave(
                         _completedWaves,
                         _stageProviderService.StagesCount))
                _survivalFlowService.OnSurvivalMilestoneReached();

            Changed?.Invoke();
        }

        public void OnSurvivalModeEntered()
        {
            _showSpawnPathPreview = true;
            ApplyPathsForWave(UpcomingWaveNumber);
            RefreshSectorVisuals();
            TryPlayPendingPathUnlockReveal();
            Changed?.Invoke();
        }

        public bool IsNextWaveStartBlocked => _pathUnlockSequenceService.IsPlaying;

        private void ApplyPathsForWave(int waveNumber)
        {
            int targetPathCount = _config.GetPathCountForWave(waveNumber);
            int newPathsNeeded = targetPathCount - _runUnlockedPathIndices.Count;

            if (newPathsNeeded <= 0)
            {
                _sectorRegistryService.SetUnlockedPathIndices(_runUnlockedPathIndices);
                return;
            }

            List<int> availablePathIndices = new List<int>();

            for (int pathIndex = 0; pathIndex < _config.MaxPathCount; pathIndex++)
            {
                if (_runUnlockedPathIndices.Contains(pathIndex) == false)
                    availablePathIndices.Add(pathIndex);
            }

            if (newPathsNeeded > 0 && _completedWaves > 0)
                _pendingUnlockRevealPathIndices.Clear();

            for (int newPathIndex = 0; newPathIndex < newPathsNeeded; newPathIndex++)
            {
                if (availablePathIndices.Count == 0)
                    break;

                int randomListIndex = Random.Range(0, availablePathIndices.Count);
                int selectedPathIndex = availablePathIndices[randomListIndex];
                _runUnlockedPathIndices.Add(selectedPathIndex);

                if (_completedWaves > 0)
                    _pendingUnlockRevealPathIndices.Add(selectedPathIndex);

                availablePathIndices.RemoveAt(randomListIndex);
            }

            _sectorRegistryService.SetUnlockedPathIndices(_runUnlockedPathIndices);
        }

        private void RefreshSectorVisuals()
        {
            SectorBootstrap sectorBootstrap = Object.FindObjectOfType<SectorBootstrap>();

            if (sectorBootstrap == null)
                return;

            SectorVisualConfig visualConfig = _configsProviderService.GetConfig<SectorVisualConfig>();
            WaveEnemyPreviewIconsConfig enemyIconsConfig =
                _configsProviderService.GetConfig<WaveEnemyPreviewIconsConfig>();

            bool hasPendingUnlockReveal = _completedWaves > 0 && _pendingUnlockRevealPathIndices.Count > 0;
            IReadOnlyList<int> pendingUnlockRevealPathIndices = hasPendingUnlockReveal
                ? _pendingUnlockRevealPathIndices
                : null;
            bool deferSpawnPathPreview = hasPendingUnlockReveal;

            if (_showSpawnPathPreview)
            {
                ClearAllEnemiesWaveRuntimeData waveData =
                    _stageProviderService.GetWaveRuntimeDataForWave(UpcomingWaveNumber);

                _waveSpawnPlanService.BuildForWave(waveData, _sectorRegistryService, UpcomingWaveNumber);
                sectorBootstrap.RefreshViews(
                    _sectorRegistryService,
                    visualConfig,
                    null,
                    false,
                    pendingUnlockRevealPathIndices);

                if (deferSpawnPathPreview == false)
                {
                    _spawnPathPreviewService.Refresh(
                        true,
                        _sectorRegistryService,
                        visualConfig,
                        enemyIconsConfig,
                        _waveSpawnPlanService.GroupPlans);
                }
                else
                {
                    _spawnPathPreviewService.ClearMarkers();
                }
            }
            else
            {
                sectorBootstrap.RefreshViews(
                    _sectorRegistryService,
                    visualConfig,
                    null,
                    false,
                    pendingUnlockRevealPathIndices);
                _spawnPathPreviewService.ClearMarkers();
            }

        }

        private void TryPlayPendingPathUnlockReveal()
        {
            if (_completedWaves <= 0 || _pendingUnlockRevealPathIndices.Count == 0)
                return;

            SectorBootstrap sectorBootstrap = Object.FindObjectOfType<SectorBootstrap>();

            if (sectorBootstrap == null)
                return;

            SectorVisualConfig visualConfig = _configsProviderService.GetConfig<SectorVisualConfig>();
            WaveEnemyPreviewIconsConfig enemyIconsConfig =
                _configsProviderService.GetConfig<WaveEnemyPreviewIconsConfig>();
            List<int> pathIndicesToReveal = new List<int>(_pendingUnlockRevealPathIndices);

            _pathUnlockSequenceService.Play(
                pathIndicesToReveal,
                sectorBootstrap,
                visualConfig,
                _sectorRegistryService,
                enemyIconsConfig,
                _waveSpawnPlanService.GroupPlans);

            _pendingUnlockRevealPathIndices.Clear();
        }
    }
}
