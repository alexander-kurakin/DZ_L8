using Assets._Project.Develop.Runtime.Configs.Gameplay.Stages;
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

        private IDisposable _stageNumberChangedDisposable;
        private IDisposable _stageResultChangedDisposable;

        public WavePreviewPresenter(
            WavePreviewView view,
            StageProviderService stageProviderService,
            WaveEnemyPreviewIconsConfig iconsConfig,
            SpellcoreProgressionService spellcoreProgressionService)
        {
            _view = view;
            _stageProviderService = stageProviderService;
            _iconsConfig = iconsConfig;
            _spellcoreProgressionService = spellcoreProgressionService;
        }

        public void Initialize()
        {
            _stageNumberChangedDisposable = _stageProviderService.CurrentStageNumber.Subscribe(OnStageNumberChanged);
            _stageResultChangedDisposable = _stageProviderService.CurrentStageResult.Subscribe(OnStageResultChanged);
            _spellcoreProgressionService.Changed += OnProgressionChanged;

            RefreshPreview();
        }

        public void Dispose()
        {
            _stageNumberChangedDisposable?.Dispose();
            _stageResultChangedDisposable?.Dispose();
            _spellcoreProgressionService.Changed -= OnProgressionChanged;
            _view.ClearIcons();
        }

        private void OnStageNumberChanged(int oldValue, int newValue) => RefreshPreview();

        private void OnStageResultChanged(StageResults oldValue, StageResults newValue) => RefreshPreview();

        private void OnProgressionChanged() => RefreshPreview();

        private void RefreshPreview()
        {
            if (_stageProviderService.HasNextStage() == false)
            {
                _view.ClearIcons();
                _view.SetVisible(false);
                return;
            }

            IReadOnlyList<WaveEnemyPreviewType> previewTypes = _stageProviderService.GetWaveEnemyPreviewTypes();

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
