using Assets._Project.Develop.Runtime.Gameplay.Features.GameplayStateBridge;
using Assets._Project.Develop.Runtime.Gameplay.Features.MainHero;
using Assets._Project.Develop.Runtime.Gameplay.Features.SpellcoreProgressionFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.UI.CommonViews;
using Assets._Project.Develop.Runtime.UI.Core;
using System;

namespace Assets._Project.Develop.Runtime.UI.Gameplay.Stages
{
    public class StagePresenter : IPresenter
    {
        private readonly IconTextView _view;
        private readonly StageProviderService _stageProviderService;
        private readonly MainHeroHolderService _mainHeroHolderService;
        private readonly SpellcoreProgressionService _spellcoreProgressionService;

        private IDisposable _currentStageNumberChangedDisposable;
        private IDisposable _currentStageResultChangedDisposable;
        private IDisposable _mainHeroRegisteredDisposable;
        private IDisposable _gameplayPhaseChangedDisposable;

        public StagePresenter(
            IconTextView view,
            StageProviderService stageProviderService,
            MainHeroHolderService mainHeroHolderService,
            SpellcoreProgressionService spellcoreProgressionService)
        {
            _view = view;
            _stageProviderService = stageProviderService;
            _mainHeroHolderService = mainHeroHolderService;
            _spellcoreProgressionService = spellcoreProgressionService;
        }

        public void Initialize()
        {
            _currentStageNumberChangedDisposable = _stageProviderService.CurrentStageNumber.Subscribe(OnStageDataChanged);
            _currentStageResultChangedDisposable = _stageProviderService.CurrentStageResult.Subscribe(OnStageResultChanged);
            _spellcoreProgressionService.Changed += OnProgressionChanged;
            _mainHeroRegisteredDisposable = _mainHeroHolderService.HeroRegistred.Subscribe(OnMainHeroRegistered);

            if (_mainHeroHolderService.MainHero != null)
                SubscribeToGameplayPhase(_mainHeroHolderService.MainHero);

            UpdateStageNumber();
        }

        public void Dispose()
        {
            _currentStageNumberChangedDisposable?.Dispose();
            _currentStageResultChangedDisposable?.Dispose();
            _mainHeroRegisteredDisposable?.Dispose();
            _gameplayPhaseChangedDisposable?.Dispose();
            _spellcoreProgressionService.Changed -= OnProgressionChanged;
        }

        private void OnMainHeroRegistered(Entity mainHero) => SubscribeToGameplayPhase(mainHero);

        private void SubscribeToGameplayPhase(Entity mainHero)
        {
            _gameplayPhaseChangedDisposable?.Dispose();
            _gameplayPhaseChangedDisposable = mainHero.GameplayPhase.Subscribe(OnGameplayPhaseChanged);
        }

        private void OnStageDataChanged(int oldValue, int newValue) => UpdateStageNumber();

        private void OnStageResultChanged(StageResults oldValue, StageResults newValue) => UpdateStageNumber();

        private void OnProgressionChanged() => UpdateStageNumber();

        private void OnGameplayPhaseChanged(GameplayStates oldState, GameplayStates newState) => UpdateStageNumber();

        private void UpdateStageNumber()
        {
            int displayWave = IsPreparationPhase()
                ? _spellcoreProgressionService.CompletedWaves
                : _stageProviderService.CurrentStageNumber.Value;

            _view.SetText($"{displayWave} / {_stageProviderService.StagesCount}");
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
