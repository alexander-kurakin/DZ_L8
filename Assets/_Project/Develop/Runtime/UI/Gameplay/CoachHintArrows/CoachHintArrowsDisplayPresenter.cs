using System;
using System.Collections.Generic;
using _Project.Develop.Runtime.UI.Gameplay;
using _Project.Develop.Runtime.UI.Gameplay.Abilities;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Sectors;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Stages;
using Assets._Project.Develop.Runtime.Gameplay.Features.Ability;
using Assets._Project.Develop.Runtime.Gameplay.Features.PlantPlacementFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature;
using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.Utilities.ConfigsManagment;
using DG.Tweening;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.UI.Gameplay.CoachHintArrows
{
    public class CoachHintArrowsDisplayPresenter : IPresenter
    {
        private const float TOAST_TOTAL_DURATION_SECONDS = 4.94f;
        private const float WORLD_ARROW_HEIGHT_OFFSET = 6f;
        private const float SCREEN_ARROW_FOLLOW_OFFSET_Y = 48f;
        private const float ABILITY_ARROW_OFFSET_Y = 160f;

        private static readonly SectorBelt[] PlantableBelts =
        {
            SectorBelt.Outer,
            SectorBelt.Middle,
            SectorBelt.Inner,
        };

        private readonly LmbFlavorToastService _lmbFlavorToastService;
        private readonly CoachHintArrowsDisplay _view;
        private readonly ViewsFactory _viewsFactory;
        private readonly GameplayPresentersFactory _presentersFactory;
        private readonly ConfigsProviderService _configsProviderService;
        private readonly SectorRegistryService _sectorRegistryService;
        private readonly PlantPlacementService _plantPlacementService;
        private readonly WaveSpawnPlanService _waveSpawnPlanService;
        private readonly SpawnPathPreviewService _spawnPathPreviewService;
        private readonly PreparationTriggerService _preparationTriggerService;

        private readonly List<HintArrowInfo> _timedArrows = new();
        private readonly List<HintArrowInfo> _persistentArrows = new();
        private readonly List<Vector3> _worldPositionBuffer = new();

        private AbilityListPresenter _abilityListPresenter;
        private IDisposable _toastSubscription;
        private IDisposable _prepareTriggerSubscription;
        private Tween _hideTween;

        public CoachHintArrowsDisplayPresenter(
            LmbFlavorToastService lmbFlavorToastService,
            CoachHintArrowsDisplay view,
            ViewsFactory viewsFactory,
            GameplayPresentersFactory presentersFactory,
            ConfigsProviderService configsProviderService,
            SectorRegistryService sectorRegistryService,
            PlantPlacementService plantPlacementService,
            WaveSpawnPlanService waveSpawnPlanService,
            SpawnPathPreviewService spawnPathPreviewService,
            PreparationTriggerService preparationTriggerService)
        {
            _lmbFlavorToastService = lmbFlavorToastService;
            _view = view;
            _viewsFactory = viewsFactory;
            _presentersFactory = presentersFactory;
            _configsProviderService = configsProviderService;
            _sectorRegistryService = sectorRegistryService;
            _plantPlacementService = plantPlacementService;
            _waveSpawnPlanService = waveSpawnPlanService;
            _spawnPathPreviewService = spawnPathPreviewService;
            _preparationTriggerService = preparationTriggerService;
        }

        public void SetAbilityListPresenter(AbilityListPresenter abilityListPresenter)
        {
            _abilityListPresenter = abilityListPresenter;
        }

        public void Initialize()
        {
            _toastSubscription = _lmbFlavorToastService.ToastRequested.Subscribe(OnToastRequested);
            _prepareTriggerSubscription = _preparationTriggerService.PrepareTriggerClicked
                .Subscribe(OnPrepareTriggerClicked);
        }

        public void LateUpdate()
        {
            UpdateArrowPositions(_timedArrows);
            UpdateArrowPositions(_persistentArrows);
        }

        public void Dispose()
        {
            _toastSubscription?.Dispose();
            _prepareTriggerSubscription?.Dispose();
            ClearTimedArrows();
            ClearPersistentArrows();
        }

        private void OnPrepareTriggerClicked(bool oldValue, bool isClicked)
        {
            if (isClicked == false)
                return;

            ClearPersistentArrows();
        }

        private void OnToastRequested(LmbFlavorToastType toastType)
        {
            ClearTimedArrows();

            bool keepUntilSphereClick = false;

            switch (toastType)
            {
                case LmbFlavorToastType.FreeMineHint:
                    ShowFreeMineHints();
                    break;

                case LmbFlavorToastType.WaveTwoTanksHint:
                    ShowEnemyPreviewHints(
                        WaveEnemyPreviewType.Tank,
                        CoachHintArrowView.PointDownDiagonalZDegrees);
                    break;

                case LmbFlavorToastType.DragonPoisonImmune:
                    ShowEnemyPreviewHints(
                        WaveEnemyPreviewType.Dragon,
                        CoachHintArrowView.PointDownDiagonalZDegrees);
                    break;

                case LmbFlavorToastType.FirstMinePlacedGoHint:
                    ShowSphereHint();
                    keepUntilSphereClick = true;
                    break;
            }

            if (keepUntilSphereClick)
                return;

            if (_timedArrows.Count == 0)
                return;

            ScheduleHideTimedArrows();
        }

        private void ShowFreeMineHints()
        {
            if (_abilityListPresenter != null
                && _abilityListPresenter.TryGetAbilitySlotView(AbilityType.PlantMine, out AbilitySlotView mineSlot))
            {
                CreateArrow(
                    null,
                    mineSlot.IconView.RectTransform,
                    CoachHintArrowView.PointDownZDegrees,
                    false);
            }

            if (_sectorRegistryService.IsInitialized == false)
                return;

            for (int beltIndex = 0; beltIndex < PlantableBelts.Length; beltIndex++)
            {
                SectorBelt belt = PlantableBelts[beltIndex];

                for (int sectorIndex = 0; sectorIndex < SectorId.SectorsPerRing; sectorIndex++)
                {
                    SectorId sectorId = new SectorId(belt, sectorIndex);

                    if (_plantPlacementService.CanPlaceOnSector(sectorId, AbilityType.PlantMine) == false)
                        continue;

                    CreateArrow(
                        _sectorRegistryService.GetAnchorPosition(sectorId),
                        null,
                        CoachHintArrowView.PointDownZDegrees,
                        false);
                }
            }
        }

        private void ShowEnemyPreviewHints(WaveEnemyPreviewType previewType, float rotationZDegrees)
        {
            _spawnPathPreviewService.CollectIconWorldPositions(
                _sectorRegistryService,
                _waveSpawnPlanService.GroupPlans,
                previewType,
                _worldPositionBuffer);

            for (int index = 0; index < _worldPositionBuffer.Count; index++)
            {
                CreateArrow(
                    _worldPositionBuffer[index],
                    null,
                    rotationZDegrees,
                    false);
            }
        }

        private void ShowSphereHint()
        {
            ClearPersistentArrows();

            ContactTriggerConfig contactTriggerConfig =
                _configsProviderService.GetConfig<ContactTriggerConfig>();

            CreateArrow(
                contactTriggerConfig.ContactTriggerStartPosition,
                null,
                CoachHintArrowView.PointDownZDegrees,
                true);
        }

        private void CreateArrow(
            Vector3? worldPosition,
            RectTransform followScreenTransform,
            float rotationZDegrees,
            bool persistUntilSphereClick)
        {
            SectorVisualConfig visualConfig = _configsProviderService.GetConfig<SectorVisualConfig>();
            Sprite arrowSprite = visualConfig.SpawnPathArrowSprite;

            if (arrowSprite == null)
                return;

            CoachHintArrowView arrowView = _viewsFactory.Create<CoachHintArrowView>(ViewIDs.CoachHintArrow);
            _view.Add(arrowView);

            arrowView.SetIcon(arrowSprite);
            arrowView.SetColor(visualConfig.SpawnPreviewOutlineColor);

            CoachHintArrowPresenter arrowPresenter = _presentersFactory.CreateCoachHintArrowPresenter(arrowView);
            arrowPresenter.Initialize(rotationZDegrees);

            bool followsScreenTransform = followScreenTransform != null;
            Vector3 resolvedWorldPosition = worldPosition ?? Vector3.zero;
            HintArrowInfo arrowInfo = new HintArrowInfo(
                arrowPresenter,
                resolvedWorldPosition,
                followScreenTransform,
                followsScreenTransform);

            if (persistUntilSphereClick)
                _persistentArrows.Add(arrowInfo);
            else
                _timedArrows.Add(arrowInfo);
        }

        private void ScheduleHideTimedArrows()
        {
            _hideTween?.Kill();

            _hideTween = DOVirtual
                .DelayedCall(TOAST_TOTAL_DURATION_SECONDS, ClearTimedArrows)
                .SetUpdate(true)
                .Play();
        }

        private void ClearTimedArrows()
        {
            _hideTween?.Kill();
            _hideTween = null;
            ReleaseArrows(_timedArrows);
        }

        private void ClearPersistentArrows()
        {
            ReleaseArrows(_persistentArrows);
        }

        private void ReleaseArrows(List<HintArrowInfo> arrows)
        {
            for (int index = 0; index < arrows.Count; index++)
            {
                HintArrowInfo arrowInfo = arrows[index];
                arrowInfo.Presenter.Dispose();
                _view.Remove(arrowInfo.Presenter.View);
                _viewsFactory.Release(arrowInfo.Presenter.View);
            }

            arrows.Clear();
        }

        private void UpdateArrowPositions(List<HintArrowInfo> arrows)
        {
            for (int index = 0; index < arrows.Count; index++)
            {
                HintArrowInfo arrowInfo = arrows[index];

                if (arrowInfo.FollowsScreenTransform)
                {
                    if (arrowInfo.FollowScreenTransform == null)
                    {
                        arrowInfo.Presenter.View.gameObject.SetActive(false);
                        continue;
                    }

                    Vector3 screenPosition = arrowInfo.FollowScreenTransform.position
                                            + Vector3.up * ABILITY_ARROW_OFFSET_Y;
                    _view.UpdateScreenPositionFor(arrowInfo.Presenter.View, screenPosition);
                    continue;
                }

                Vector3 worldPosition = arrowInfo.WorldPosition + Vector3.up * WORLD_ARROW_HEIGHT_OFFSET;
                _view.UpdatePositionFor(arrowInfo.Presenter.View, worldPosition);

                if (arrowInfo.Presenter.View.gameObject.activeSelf)
                {
                    arrowInfo.Presenter.View.transform.position +=
                        Vector3.up * SCREEN_ARROW_FOLLOW_OFFSET_Y;
                }
            }
        }

        private class HintArrowInfo
        {
            public HintArrowInfo(
                CoachHintArrowPresenter presenter,
                Vector3 worldPosition,
                RectTransform followScreenTransform,
                bool followsScreenTransform)
            {
                Presenter = presenter;
                WorldPosition = worldPosition;
                FollowScreenTransform = followScreenTransform;
                FollowsScreenTransform = followsScreenTransform;
            }

            public CoachHintArrowPresenter Presenter { get; }

            public Vector3 WorldPosition { get; }

            public RectTransform FollowScreenTransform { get; }

            public bool FollowsScreenTransform { get; }
        }
    }
}
