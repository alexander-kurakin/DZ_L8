using Assets._Project.Develop.Runtime.Configs.Gameplay.Throw;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.MainHero;
using Assets._Project.Develop.Runtime.UI.CommonViews;
using Assets._Project.Develop.Runtime.UI.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Develop.Runtime.UI.Gameplay.ThrowCharge
{
    public class EntitiesThrowChargeDisplayPresenter : ILateUpdatablePresenter, IPresenter
    {
        private readonly EntitiesLifeContext _entitiesLifeContext;
        private readonly EntitiesThrowChargeDisplay _view;
        private readonly ViewsFactory _viewsFactory;
        private readonly GameplayPresentersFactory _presentersFactory;

        private readonly Dictionary<Entity, EntityThrowChargeBarInfo> _entityToThrowChargeBarInfo = new();

        private bool _isInitialized;

        public EntitiesThrowChargeDisplayPresenter(
            EntitiesLifeContext entitiesLifeContext,
            EntitiesThrowChargeDisplay view,
            ViewsFactory viewsFactory,
            GameplayPresentersFactory presentersFactory)
        {
            _entitiesLifeContext = entitiesLifeContext;
            _view = view;
            _viewsFactory = viewsFactory;
            _presentersFactory = presentersFactory;
        }

        public void Initialize()
        {
            if (_isInitialized)
                return;

            _isInitialized = true;

            _entitiesLifeContext.Added += OnEntityAdded;
            _entitiesLifeContext.Released += OnEntityReleased;

            foreach (Entity entity in _entitiesLifeContext.Entities)
                OnEntityAdded(entity);
        }

        public void Dispose()
        {
            _entitiesLifeContext.Added -= OnEntityAdded;
            _entitiesLifeContext.Released -= OnEntityReleased;

            foreach (EntityThrowChargeBarInfo info in _entityToThrowChargeBarInfo.Values)
                DisposeFor(info);

            _entityToThrowChargeBarInfo.Clear();
        }

        public void LateUpdate()
        {
            foreach (KeyValuePair<Entity, EntityThrowChargeBarInfo> info in _entityToThrowChargeBarInfo)
                _view.UpdatePositionFor(info.Value.ThrowChargePresenter.BarRectTransform, info.Value.ThrowChargeBarPoint.position);
        }

        private void OnEntityReleased(Entity entity)
        {
            if (_entityToThrowChargeBarInfo.ContainsKey(entity))
                RemoveThrowChargeBarFor(entity);
        }

        private void OnEntityAdded(Entity entity)
        {
            if (_entityToThrowChargeBarInfo.ContainsKey(entity))
                return;

            if (entity.TryGetThrowChargeBarPoint(out Transform throwChargeBarPoint) == false)
                return;

            if (entity.HasComponent<IsMainHero>() == false)
                return;

            Bar throwChargeBarView = _viewsFactory.Create<Bar>(ViewIDs.ThrowChargeBar);
            _view.Add(throwChargeBarView.GetComponent<RectTransform>());

            EntityThrowChargePresenter entityThrowChargePresenter =
                _presentersFactory.CreateEntityThrowChargePresenter(entity, throwChargeBarView);

            entityThrowChargePresenter.Initialize();

            _entityToThrowChargeBarInfo.Add(
                entity,
                new EntityThrowChargeBarInfo(throwChargeBarPoint, entityThrowChargePresenter));
        }

        private void RemoveThrowChargeBarFor(Entity entity)
        {
            EntityThrowChargeBarInfo info = _entityToThrowChargeBarInfo[entity];
            DisposeFor(info);
            _entityToThrowChargeBarInfo.Remove(entity);
        }

        private void DisposeFor(EntityThrowChargeBarInfo info)
        {
            _view.Remove(info.ThrowChargePresenter.BarRectTransform);
            _viewsFactory.Release(info.ThrowChargePresenter.Bar);
            info.ThrowChargePresenter.Dispose();
        }

        private class EntityThrowChargeBarInfo
        {
            public EntityThrowChargeBarInfo(
                Transform throwChargeBarPoint,
                EntityThrowChargePresenter throwChargePresenter)
            {
                ThrowChargeBarPoint = throwChargeBarPoint;
                ThrowChargePresenter = throwChargePresenter;
            }

            public Transform ThrowChargeBarPoint { get; }

            public EntityThrowChargePresenter ThrowChargePresenter { get; }
        }
    }
}
