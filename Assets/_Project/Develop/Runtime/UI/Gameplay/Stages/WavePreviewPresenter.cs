using Assets._Project.Develop.Runtime.Configs.Gameplay.Stages;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.GameplayStateBridge;
using Assets._Project.Develop.Runtime.Gameplay.Features.MainHero;
using Assets._Project.Develop.Runtime.Gameplay.Features.SpellcoreProgressionFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature;
using Assets._Project.Develop.Runtime.UI.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.UI.Gameplay.Stages
{
    public class WavePreviewPresenter : IPresenter
    {
        private readonly WavePreviewView _view;
        private readonly StageProviderService _stageProviderService;
        private readonly WaveEnemyPreviewIconsConfig _iconsConfig;
        private readonly SpellcoreProgressionService _spellcoreProgressionService;
        private readonly MainHeroHolderService _mainHeroHolderService;

        private IDisposable _stageNumberChangedDisposable;
        private IDisposable _stageResultChangedDisposable;
        private IDisposable _mainHeroRegisteredDisposable;
        private IDisposable _gameplayPhaseChangedDisposable;

        public WavePreviewPresenter(
            WavePreviewView view,
            StageProviderService stageProviderService,
            WaveEnemyPreviewIconsConfig iconsConfig,
            SpellcoreProgressionService spellcoreProgressionService,
            MainHeroHolderService mainHeroHolderService)
        {
            _view = view;
            _stageProviderService = stageProviderService;
            _iconsConfig = iconsConfig;
            _spellcoreProgressionService = spellcoreProgressionService;
            _mainHeroHolderService = mainHeroHolderService;
        }

        public void Initialize()
        {
            _stageNumberChangedDisposable = _stageProviderService.CurrentStageNumber.Subscribe(OnStageDataChanged);
            _stageResultChangedDisposable = _stageProviderService.CurrentStageResult.Subscribe(OnStageResultChanged);
            _spellcoreProgressionService.Changed += OnProgressionChanged;
            _mainHeroRegisteredDisposable = _mainHeroHolderService.HeroRegistred.Subscribe(OnMainHeroRegistered);

            if (_mainHeroHolderService.MainHero != null)
                SubscribeToGameplayPhase(_mainHeroHolderService.MainHero);

            RefreshPreview();
        }

        public void Dispose()
        {
            _stageNumberChangedDisposable?.Dispose();
            _stageResultChangedDisposable?.Dispose();
            _mainHeroRegisteredDisposable?.Dispose();
            _gameplayPhaseChangedDisposable?.Dispose();
            _spellcoreProgressionService.Changed -= OnProgressionChanged;
            _view.ClearIcons();
        }

        private void OnMainHeroRegistered(Entity mainHero) => SubscribeToGameplayPhase(mainHero);

        private void SubscribeToGameplayPhase(Entity mainHero)
        {
            _gameplayPhaseChangedDisposable?.Dispose();
            _gameplayPhaseChangedDisposable = mainHero.GameplayPhase.Subscribe(OnGameplayPhaseChanged);
        }

        private void OnStageDataChanged(int oldValue, int newValue) => RefreshPreview();

        private void OnStageResultChanged(StageResults oldValue, StageResults newValue) => RefreshPreview();

        private void OnProgressionChanged() => RefreshPreview();

        private void OnGameplayPhaseChanged(GameplayStates oldState, GameplayStates newState) => RefreshPreview();

        private void RefreshPreview()
        {
            if (IsPreparationPhase() == false)
            {
                _view.ClearIcons();
                _view.SetVisible(false);
                return;
            }

            if (_spellcoreProgressionService.CompletedWaves >= _stageProviderService.StagesCount)
            {
                _view.ClearIcons();
                _view.SetVisible(false);
                return;
            }

            IReadOnlyList<WaveEnemyPreviewType> previewTypes =
                _stageProviderService.GetWaveEnemyPreviewTypesForWave(
                    _spellcoreProgressionService.UpcomingWaveNumber);

            if (previewTypes.Count == 0)
            {
                _view.ClearIcons();
                _view.SetVisible(false);
                return;
            }

            List<Sprite> sprites = new List<Sprite>(previewTypes.Count);

            foreach (WaveEnemyPreviewType previewType in previewTypes)
                sprites.Add(_iconsConfig.GetSpriteFor(previewType));

            _view.SetIcons(sprites);
            _view.SetVisible(true);
        }

        private bool IsPreparationPhase()
        {
            Entity mainHero = _mainHeroHolderService.MainHero;

            if (mainHero == null)
                return true;

            return mainHero.GameplayPhase.Value == GameplayStates.Preparation;
        }
    }
}
