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
        private IDisposable _gameplayPhaseChangedDisposable;
        private IDisposable _heroRegisteredDisposable;

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
            _stageNumberChangedDisposable = _stageProviderService.CurrentStageNumber.Subscribe(OnStageNumberChanged);
            _spellcoreProgressionService.Changed += OnProgressionChanged;

            if (_mainHeroHolderService.MainHero != null)
                SubscribeToGameplayPhase(_mainHeroHolderService.MainHero);
            else
                _heroRegisteredDisposable = _mainHeroHolderService.HeroRegistred.Subscribe(OnHeroRegistered);

            RefreshPreview();
        }

        public void Dispose()
        {
            _stageNumberChangedDisposable?.Dispose();
            _gameplayPhaseChangedDisposable?.Dispose();
            _heroRegisteredDisposable?.Dispose();
            _spellcoreProgressionService.Changed -= OnProgressionChanged;
            _view.ClearIcons();
        }

        private void OnHeroRegistered(Entity mainHero)
        {
            _heroRegisteredDisposable?.Dispose();
            _heroRegisteredDisposable = null;
            SubscribeToGameplayPhase(mainHero);
            RefreshPreview();
        }

        private void SubscribeToGameplayPhase(Entity mainHero)
        {
            _gameplayPhaseChangedDisposable?.Dispose();
            _gameplayPhaseChangedDisposable = mainHero.GameplayPhase.Subscribe(OnGameplayPhaseChanged);
        }

        private void OnStageNumberChanged(int oldValue, int newValue) => RefreshPreview();

        private void OnProgressionChanged() => RefreshPreview();

        private void OnGameplayPhaseChanged(GameplayStates oldState, GameplayStates newState) => RefreshPreview();

        private void RefreshPreview()
        {
            Entity mainHero = _mainHeroHolderService.MainHero;

            if (mainHero != null && mainHero.GameplayPhase.Value == GameplayStates.StageProcess)
            {
                _view.SetVisible(false);
                return;
            }

            if (_stageProviderService.HasNextStage() == false)
            {
                _view.ClearIcons();
                _view.SetVisible(false);
                return;
            }

            IReadOnlyList<WaveEnemyPreviewType> previewTypes = _stageProviderService.GetUpcomingWaveEnemyPreviewTypes();

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
    }
}
