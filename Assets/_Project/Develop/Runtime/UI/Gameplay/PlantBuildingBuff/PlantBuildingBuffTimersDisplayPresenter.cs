using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.PlantBuildingBuff;
using Assets._Project.Develop.Runtime.UI.CommonViews;
using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System;
using System.Collections.Generic;
using _Project.Develop.Runtime.UI.Gameplay;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.UI.Gameplay.PlantBuildingBuff
{
    public class PlantBuildingBuffTimersDisplayPresenter : IPresenter
    {
        private const float LABEL_WORLD_OFFSET_Y = 4f;

        private readonly PlantBuildingBuffService _plantBuildingBuffService;
        private readonly PlantBuildingBuffTimersDisplay _view;
        private readonly ViewsFactory _viewsFactory;
        private readonly GameplayPresentersFactory _presentersFactory;

        private readonly Dictionary<Entity, BuffTimerInfo> _timersByPlant = new();

        public PlantBuildingBuffTimersDisplayPresenter(
            PlantBuildingBuffService plantBuildingBuffService,
            PlantBuildingBuffTimersDisplay view,
            ViewsFactory viewsFactory,
            GameplayPresentersFactory presentersFactory)
        {
            _plantBuildingBuffService = plantBuildingBuffService;
            _view = view;
            _viewsFactory = viewsFactory;
            _presentersFactory = presentersFactory;
        }

        public void Initialize()
        {
            _plantBuildingBuffService.BuffTimerStarted += OnBuffTimerStarted;
            _plantBuildingBuffService.BuffTimerEnded += OnBuffTimerEnded;
        }

        public void LateUpdate()
        {
            foreach (KeyValuePair<Entity, BuffTimerInfo> entry in _timersByPlant)
            {
                Entity plantEntity = entry.Key;

                if (plantEntity.TryGetTransform(out Transform plantTransform) == false)
                    continue;

                Vector3 worldPosition = plantTransform.position + Vector3.up * LABEL_WORLD_OFFSET_Y;
                _view.UpdatePositionFor(entry.Value.CountdownPresenter.View, worldPosition);
            }
        }

        public void Dispose()
        {
            _plantBuildingBuffService.BuffTimerStarted -= OnBuffTimerStarted;
            _plantBuildingBuffService.BuffTimerEnded -= OnBuffTimerEnded;

            foreach (BuffTimerInfo timerInfo in _timersByPlant.Values)
                DisposeTimer(timerInfo);

            _timersByPlant.Clear();
        }

        private void OnBuffTimerStarted(Entity plantEntity)
        {
            if (_timersByPlant.ContainsKey(plantEntity))
                return;

            if (_plantBuildingBuffService.TryGetRemainingSeconds(plantEntity, out ReactiveVariable<float> remainingSeconds) == false)
                return;

            TextView timerView = _viewsFactory.Create<TextView>(ViewIDs.BuildingBuffTimerTextView);
            _view.Add(timerView);

            PlantBuildingBuffCountdownPresenter countdownPresenter =
                _presentersFactory.CreatePlantBuildingBuffCountdownPresenter(remainingSeconds, timerView);
            countdownPresenter.Initialize();

            _timersByPlant.Add(plantEntity, new BuffTimerInfo(countdownPresenter));
        }

        private void OnBuffTimerEnded(Entity plantEntity)
        {
            if (_timersByPlant.TryGetValue(plantEntity, out BuffTimerInfo timerInfo) == false)
                return;

            DisposeTimer(timerInfo);
            _timersByPlant.Remove(plantEntity);
        }

        private void DisposeTimer(BuffTimerInfo timerInfo)
        {
            timerInfo.CountdownPresenter.Dispose();
            _view.Remove(timerInfo.CountdownPresenter.View);
            _viewsFactory.Release(timerInfo.CountdownPresenter.View);
        }

        private class BuffTimerInfo
        {
            public BuffTimerInfo(PlantBuildingBuffCountdownPresenter countdownPresenter)
            {
                CountdownPresenter = countdownPresenter;
            }

            public PlantBuildingBuffCountdownPresenter CountdownPresenter { get; }
        }
    }
}
