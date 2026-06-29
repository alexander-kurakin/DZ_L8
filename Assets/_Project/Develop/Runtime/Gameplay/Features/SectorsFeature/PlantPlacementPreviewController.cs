using System;
using System.Collections.Generic;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.Ability;
using Assets._Project.Develop.Runtime.Gameplay.Features.GameplayStateBridge;
using Assets._Project.Develop.Runtime.Gameplay.Features.MainHero;
using Assets._Project.Develop.Runtime.Gameplay.Features.PlantPlacementFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.SpellcoreProgressionFeature;
using Assets._Project.Develop.Runtime.Utilities.Reactive;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature
{
    public class PlantPlacementPreviewController : IDisposable
    {
        private readonly PlantPlacementPreviewService _previewService;
        private readonly SectorRegistryService _sectorRegistryService;
        private readonly PlantPlacementService _plantPlacementService;
        private readonly MainHeroHolderService _mainHeroHolderService;
        private readonly SpellcoreProgressionService _spellcoreProgressionService;
        private readonly PathUnlockSequenceService _pathUnlockSequenceService;

        private static readonly SectorBelt[] PLANTABLE_BELTS =
        {
            SectorBelt.Inner,
            SectorBelt.Middle,
            SectorBelt.Outer,
        };

        private Entity _mainHero;
        private IDisposable _heroRegisteredSubscription;
        private IDisposable _activeAbilitySubscription;
        private IDisposable _gameplayPhaseSubscription;

        public PlantPlacementPreviewController(
            PlantPlacementPreviewService previewService,
            SectorRegistryService sectorRegistryService,
            PlantPlacementService plantPlacementService,
            MainHeroHolderService mainHeroHolderService,
            SpellcoreProgressionService spellcoreProgressionService,
            PathUnlockSequenceService pathUnlockSequenceService)
        {
            _previewService = previewService;
            _sectorRegistryService = sectorRegistryService;
            _plantPlacementService = plantPlacementService;
            _mainHeroHolderService = mainHeroHolderService;
            _spellcoreProgressionService = spellcoreProgressionService;
            _pathUnlockSequenceService = pathUnlockSequenceService;

            _plantPlacementService.PlacementChanged += OnPlacementChanged;
            _spellcoreProgressionService.Changed += OnProgressionChanged;
            _pathUnlockSequenceService.UnlockSequenceStarted += OnUnlockSequenceStarted;
            _pathUnlockSequenceService.RevealPlantMarkersForBelt += OnRevealPlantMarkersForBelt;
            _pathUnlockSequenceService.UnlockSequenceCompleted += OnUnlockSequenceCompleted;
            _heroRegisteredSubscription = _mainHeroHolderService.HeroRegistred.Subscribe(OnHeroRegistered);

            if (_mainHeroHolderService.MainHero != null)
                OnHeroRegistered(_mainHeroHolderService.MainHero);
        }

        public void Dispose()
        {
            _plantPlacementService.PlacementChanged -= OnPlacementChanged;
            _spellcoreProgressionService.Changed -= OnProgressionChanged;
            _pathUnlockSequenceService.UnlockSequenceStarted -= OnUnlockSequenceStarted;
            _pathUnlockSequenceService.RevealPlantMarkersForBelt -= OnRevealPlantMarkersForBelt;
            _pathUnlockSequenceService.UnlockSequenceCompleted -= OnUnlockSequenceCompleted;
            _heroRegisteredSubscription?.Dispose();
            UnbindFromHero();
            _previewService.ClearMarkers();
        }

        private void OnHeroRegistered(Entity mainHero)
        {
            UnbindFromHero();
            _mainHero = mainHero;
            _activeAbilitySubscription = _mainHero.AbilityUserActiveAbility.Subscribe(OnActiveAbilityChanged);
            _gameplayPhaseSubscription = _mainHero.GameplayPhase.Subscribe(OnGameplayPhaseChanged);
            RefreshPreview();
        }

        private void UnbindFromHero()
        {
            _activeAbilitySubscription?.Dispose();
            _activeAbilitySubscription = null;
            _gameplayPhaseSubscription?.Dispose();
            _gameplayPhaseSubscription = null;
            _mainHero = null;
        }

        private void OnActiveAbilityChanged(AbilityType oldAbility, AbilityType newAbility)
        {
            RefreshPreview();
        }

        private void OnGameplayPhaseChanged(GameplayStates oldPhase, GameplayStates newPhase)
        {
            RefreshPreview();
        }

        private void OnPlacementChanged()
        {
            RefreshPreview();
        }

        private void OnProgressionChanged()
        {
            RefreshPreview();
        }

        private void OnUnlockSequenceStarted(IReadOnlyList<int> revealingPathIndices)
        {
            RefreshStablePathMarkers();
        }

        private void OnRevealPlantMarkersForBelt(SectorBelt belt, IReadOnlyList<int> revealingPathIndices)
        {
            if (TryGetPreviewContext(out bool showPreview, out AbilityType activeAbility) == false)
                return;

            if (showPreview == false)
                return;

            for (int pathIndex = 0; pathIndex < revealingPathIndices.Count; pathIndex++)
            {
                _previewService.EnsureMarkersForPath(
                    _sectorRegistryService,
                    _plantPlacementService,
                    activeAbility,
                    revealingPathIndices[pathIndex],
                    belt);
            }
        }

        private void OnUnlockSequenceCompleted()
        {
            RefreshPreview();
        }

        private void RefreshPreview()
        {
            if (_mainHero == null)
            {
                _previewService.ClearMarkers();
                return;
            }

            if (_pathUnlockSequenceService.IsPlaying)
            {
                RefreshStablePathMarkers();
                return;
            }

            if (TryGetPreviewContext(out bool showPreview, out AbilityType activeAbility) == false)
                return;

            if (showPreview == false)
                _previewService.ClearHoverCross();

            _previewService.Refresh(
                showPreview,
                _sectorRegistryService,
                _plantPlacementService,
                activeAbility);
        }

        private void RefreshStablePathMarkers()
        {
            if (TryGetPreviewContext(out bool showPreview, out AbilityType activeAbility) == false)
                return;

            _previewService.ClearMarkers();

            if (showPreview == false)
                return;

            IReadOnlyList<int> unlockedPathIndices = _sectorRegistryService.UnlockedPathIndices;
            IReadOnlyList<int> revealingPathIndices = _pathUnlockSequenceService.RevealingPathIndices;

            for (int pathListIndex = 0; pathListIndex < unlockedPathIndices.Count; pathListIndex++)
            {
                int pathIndex = unlockedPathIndices[pathListIndex];

                if (IsRevealingPath(pathIndex, revealingPathIndices))
                {
                    EnsureRevealedMarkersForPath(pathIndex, activeAbility);
                    continue;
                }

                _previewService.EnsureMarkersForPath(
                    _sectorRegistryService,
                    _plantPlacementService,
                    activeAbility,
                    pathIndex,
                    null);
            }
        }

        private void EnsureRevealedMarkersForPath(int pathIndex, AbilityType activeAbility)
        {
            for (int beltIndex = 0; beltIndex < PLANTABLE_BELTS.Length; beltIndex++)
            {
                SectorBelt belt = PLANTABLE_BELTS[beltIndex];

                if (_pathUnlockSequenceService.IsPlantMarkerBeltRevealed(belt) == false)
                    continue;

                _previewService.EnsureMarkersForPath(
                    _sectorRegistryService,
                    _plantPlacementService,
                    activeAbility,
                    pathIndex,
                    belt);
            }
        }

        private bool TryGetPreviewContext(out bool showPreview, out AbilityType activeAbility)
        {
            showPreview = false;
            activeAbility = default;

            if (_mainHero == null)
                return false;

            GameplayStates gameplayPhase = _mainHero.GameplayPhase.Value;
            activeAbility = _mainHero.AbilityUserActiveAbility.Value;
            bool isPreviewPhase = gameplayPhase == GameplayStates.Preparation
                                  || gameplayPhase == GameplayStates.StageProcess;
            showPreview = isPreviewPhase && PlantPlacementService.IsPlantAbility(activeAbility);
            return true;
        }

        private static bool IsRevealingPath(int pathIndex, IReadOnlyList<int> revealingPathIndices)
        {
            if (revealingPathIndices == null)
                return false;

            for (int index = 0; index < revealingPathIndices.Count; index++)
            {
                if (revealingPathIndices[index] == pathIndex)
                    return true;
            }

            return false;
        }
    }
}
