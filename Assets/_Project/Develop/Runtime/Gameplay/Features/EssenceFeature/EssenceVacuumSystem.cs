using Assets._Project.Develop.Runtime.Configs.Gameplay.Essence;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Gameplay.Features.MainHero;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.EssenceFeature
{
    public class EssenceVacuumSystem : IInitializableSystem, IUpdatableSystem, IDisposableSystem
    {
        private readonly EssenceConfig _essenceConfig;
        private readonly RunEssenceService _runEssenceService;
        private readonly MainHeroHolderService _mainHeroHolderService;

        private Transform _transform;
        private ReactiveVariable<int> _essenceAmount;
        private ReactiveVariable<float> _remainingHoverUnlockTime;
        private ReactiveVariable<bool> _canAcceptHover;
        private ReactiveVariable<bool> _isVacuuming;
        private ReactiveVariable<bool> _isCollected;
        private ReactiveEvent _hoverReadyEvent;
        private ReactiveEvent _vacuumStartedEvent;
        private ReactiveEvent _collectedEvent;
        private Collider _hoverCollider;
        private IDisposable _startVacuumRequestDisposable;

        public EssenceVacuumSystem(
            EssenceConfig essenceConfig,
            RunEssenceService runEssenceService,
            MainHeroHolderService mainHeroHolderService)
        {
            _essenceConfig = essenceConfig;
            _runEssenceService = runEssenceService;
            _mainHeroHolderService = mainHeroHolderService;
        }

        public void OnInit(Entity entity)
        {
            _transform = entity.Transform;
            _essenceAmount = entity.EssenceAmount;
            _remainingHoverUnlockTime = entity.EssenceHoverUnlockRemainingTime;
            _canAcceptHover = entity.EssenceCanAcceptHover;
            _isVacuuming = entity.EssenceIsVacuuming;
            _isCollected = entity.EssenceIsCollected;
            _hoverReadyEvent = entity.EssenceHoverReadyEvent;
            _vacuumStartedEvent = entity.EssenceVacuumStartedEvent;
            _collectedEvent = entity.EssenceCollectedEvent;
            _hoverCollider = entity.EssenceHoverCollider;

            _startVacuumRequestDisposable = entity.EssenceStartVacuumRequest.Subscribe(OnStartVacuumRequested);
        }

        public void OnUpdate(float deltaTime)
        {
            if (_isCollected.Value)
                return;

            if (_isVacuuming.Value == false)
                return;

            if (TryGetTowerCollectPosition(out Vector3 towerCollectPosition) == false)
                return;

            _transform.position = Vector3.MoveTowards(
                _transform.position,
                towerCollectPosition,
                _essenceConfig.VacuumMoveSpeed * deltaTime);

            float collectDistance = GetFlatDistance(_transform.position, towerCollectPosition);

            if (collectDistance <= _essenceConfig.TowerCollectRadius)
                Collect();
        }

        public void OnDispose()
        {
            _startVacuumRequestDisposable?.Dispose();
        }

        private void OnStartVacuumRequested()
        {
            if (_isCollected.Value)
                return;

            if (_isVacuuming.Value)
                return;

            ForceUnlockHoverIfNeeded();

            _isVacuuming.Value = true;
            _vacuumStartedEvent.Invoke();
        }

        private void ForceUnlockHoverIfNeeded()
        {
            if (_canAcceptHover.Value)
                return;

            _remainingHoverUnlockTime.Value = 0f;
            _canAcceptHover.Value = true;

            if (_hoverCollider != null)
                _hoverCollider.enabled = true;

            _hoverReadyEvent.Invoke();
        }

        private void Collect()
        {
            if (_isCollected.Value)
                return;

            int collectedAmount = Mathf.FloorToInt(_essenceAmount.Value * _essenceConfig.TowerEatFraction);
            _runEssenceService.Add(collectedAmount);

            _collectedEvent.Invoke();
            _isCollected.Value = true;
        }

        private bool TryGetTowerCollectPosition(out Vector3 towerCollectPosition)
        {
            towerCollectPosition = Vector3.zero;

            Entity mainHero = _mainHeroHolderService.MainHero;

            if (mainHero == null)
                return false;

            if (mainHero.TryGetTransform(out Transform towerTransform) == false)
                return false;

            if (towerTransform == null)
                return false;

            towerCollectPosition = towerTransform.position;
            towerCollectPosition.y += _essenceConfig.TowerCollectHeightOffset;
            return true;
        }

        private static float GetFlatDistance(Vector3 firstPoint, Vector3 secondPoint)
        {
            firstPoint.y = 0f;
            secondPoint.y = 0f;
            return Vector3.Distance(firstPoint, secondPoint);
        }
    }
}
