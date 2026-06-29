using System;
using System.Collections.Generic;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Sectors;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Spellcore;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Stages;
using Assets._Project.Develop.Runtime.Gameplay.Features.Ability;
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
        private readonly ConfigsProviderService _configsProviderService;
        private readonly SpawnPathPreviewService _spawnPathPreviewService;
        private readonly WaveSpawnPlanService _waveSpawnPlanService;
        private readonly PathUnlockSequenceService _pathUnlockSequenceService;

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
            ConfigsProviderService configsProviderService,
            SpawnPathPreviewService spawnPathPreviewService,
            WaveSpawnPlanService waveSpawnPlanService,
            PathUnlockSequenceService pathUnlockSequenceService)
        {
            _config = config;
            _sectorRegistryService = sectorRegistryService;
            _stageProviderService = stageProviderService;
            _configsProviderService = configsProviderService;
            _spawnPathPreviewService = spawnPathPreviewService;
            _waveSpawnPlanService = waveSpawnPlanService;
            _pathUnlockSequenceService = pathUnlockSequenceService;
        }

        public event Action Changed;

        public int CompletedWaves => _completedWaves;

        public int FreeMinesRemaining => _freeMinesRemaining;

        public void InitializeForRun()
        {
            _completedWaves = 0;
            _freeMinesRemaining = 0;
            _runUnlockedPathIndices.Clear();
            _pendingUnlockRevealPathIndices.Clear();
            _stageCompletedSubscription = _stageProviderService.StageCompleted.Subscribe(OnWaveCompleted);

            ApplyPathsForWave(GetUpcomingWaveNumber());
            RefreshSectorVisuals();
            Changed?.Invoke();
        }

        public void OnPreparationEntered()
        {
            _showSpawnPathPreview = true;
            ApplyPathsForWave(GetUpcomingWaveNumber());
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
                    return _completedWaves >= 1;

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
                _freeMinesRemaining = _config.StarterFreeMines;

            Changed?.Invoke();
        }

        private int GetUpcomingWaveNumber() => _completedWaves + 1;

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

            for (int pathIndex = 0; pathIndex < SectorId.SectorsPerRing; pathIndex++)
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
                int upcomingWaveNumber = GetUpcomingWaveNumber();
                ClearAllEnemiesStageConfig stageConfig =
                    _stageProviderService.GetClearAllEnemiesStageConfigForWave(upcomingWaveNumber);

                _waveSpawnPlanService.BuildForWave(stageConfig, _sectorRegistryService);
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
