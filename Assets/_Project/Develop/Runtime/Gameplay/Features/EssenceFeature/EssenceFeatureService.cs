using _Project.Develop.Runtime.Gameplay.Features.Input;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Essence;
using Assets._Project.Develop.Runtime.Configs.Gameplay.MouseConfig;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Stages;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.MainHero;
using Assets._Project.Develop.Runtime.Gameplay.Features.TeamsFeature;
using Assets._Project.Develop.Runtime.Utilities.ConfigsManagment;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System;
using System.Collections.Generic;
using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.EssenceFeature
{
    public class EssenceFeatureService : IDisposable
    {
        private const float PICKUP_COLOR_RED = 0.45f;
        private const float PICKUP_COLOR_GREEN = 0.95f;
        private const float PICKUP_COLOR_BLUE = 1f;

        private readonly EssenceConfig _essenceConfig;
        private readonly RunEssenceService _runEssenceService;
        private readonly EntitiesLifeContext _entitiesLifeContext;
        private readonly MainHeroHolderService _mainHeroHolderService;
        private readonly IMouseInputService _mouseInputService;
        private readonly MouseRaycastService _mouseRaycastService;
        private readonly RaycastConfig _raycastConfig;

        private readonly List<EssencePickupView> _activePickups = new();
        private readonly Dictionary<Entity, IDisposable> _enemyDeathSubscriptions = new();

        public EssenceFeatureService(
            ConfigsProviderService configsProviderService,
            RunEssenceService runEssenceService,
            EntitiesLifeContext entitiesLifeContext,
            MainHeroHolderService mainHeroHolderService,
            IMouseInputService mouseInputService,
            MouseRaycastService mouseRaycastService)
        {
            _essenceConfig = configsProviderService.GetConfig<EssenceConfig>();
            _runEssenceService = runEssenceService;
            _entitiesLifeContext = entitiesLifeContext;
            _mainHeroHolderService = mainHeroHolderService;
            _mouseInputService = mouseInputService;
            _mouseRaycastService = mouseRaycastService;
            _raycastConfig = configsProviderService.GetConfig<RaycastConfig>();

            _entitiesLifeContext.Added += OnEntityAdded;
            _entitiesLifeContext.Released += OnEntityReleased;
        }

        public void InitializeForRun()
        {
            ClearActivePickups();
            _runEssenceService.InitializeForRun(_essenceConfig);
        }

        public void ClearForNewRun()
        {
            ClearActivePickups();
            _runEssenceService.ClearForNewRun();
        }

        public void Update(float deltaTime)
        {
            if (_activePickups.Count == 0)
                return;

            Vector3 towerPosition = GetTowerPosition();
            EssencePickupView hoveredPickup = TryGetHoveredPickup();

            for (int pickupIndex = _activePickups.Count - 1; pickupIndex >= 0; pickupIndex--)
            {
                EssencePickupView pickup = _activePickups[pickupIndex];

                if (pickup == null)
                {
                    _activePickups.RemoveAt(pickupIndex);
                    continue;
                }

                pickup.TickHoverLock(deltaTime);

                if (pickup.IsVacuuming == false && pickup.CanAcceptHover && pickup == hoveredPickup)
                    pickup.StartVacuuming();

                if (pickup.IsVacuuming)
                {
                    pickup.MoveTowards(towerPosition, _essenceConfig.VacuumMoveSpeed, deltaTime);

                    float collectDistance = GetFlatDistance(pickup.transform.position, towerPosition);

                    if (collectDistance <= _essenceConfig.TowerCollectRadius)
                        CollectPickup(pickupIndex);
                }
            }
        }

        public void Dispose()
        {
            _entitiesLifeContext.Added -= OnEntityAdded;
            _entitiesLifeContext.Released -= OnEntityReleased;

            foreach (KeyValuePair<Entity, IDisposable> subscription in _enemyDeathSubscriptions)
                subscription.Value.Dispose();

            _enemyDeathSubscriptions.Clear();
            ClearActivePickups();
        }

        private void OnEntityAdded(Entity entity)
        {
            if (entity.TryGetTeam(out ReactiveVariable<Teams> team) == false)
                return;

            if (team.Value != Teams.Enemies)
                return;

            if (entity.TryGetComponent(out EnemyWavePreviewType previewTypeComponent) == false)
                return;

            WaveEnemyPreviewType previewType = previewTypeComponent.Value;

            IDisposable deathSubscription = entity.IsDead.Subscribe((bool oldValue, bool isDead) =>
            {
                if (oldValue == false && isDead)
                    SpawnPickup(entity, previewType);
            });

            _enemyDeathSubscriptions[entity] = deathSubscription;
        }

        private void OnEntityReleased(Entity entity)
        {
            if (_enemyDeathSubscriptions.TryGetValue(entity, out IDisposable subscription) == false)
                return;

            subscription.Dispose();
            _enemyDeathSubscriptions.Remove(entity);
        }

        private void SpawnPickup(Entity enemy, WaveEnemyPreviewType previewType)
        {
            if (enemy.TryGetTransform(out Transform enemyTransform) == false)
                return;

            int dropAmount = _essenceConfig.GetDropAmountFor(previewType);

            if (dropAmount <= 0)
                return;

            Vector3 spawnPosition = enemyTransform.position;

            GameObject pickupObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pickupObject.name = "EssencePickup";

            Renderer pickupRenderer = pickupObject.GetComponent<Renderer>();
            pickupRenderer.material.color = new Color(PICKUP_COLOR_RED, PICKUP_COLOR_GREEN, PICKUP_COLOR_BLUE);

            EssencePickupView pickup = pickupObject.AddComponent<EssencePickupView>();
            pickup.Initialize(dropAmount, spawnPosition, _essenceConfig.HoverUnlockDelay);
            _activePickups.Add(pickup);
        }

        private EssencePickupView TryGetHoveredPickup()
        {
            Vector2 pointerScreenPosition = _mouseInputService.PointerScreenPosition;
            Ray ray = Camera.main.ScreenPointToRay(pointerScreenPosition);
            RaycastHit[] hits = Physics.RaycastAll(ray, _raycastConfig.MouseRaycastDistance);

            EssencePickupView closestPickup = null;
            float closestHitDistance = float.MaxValue;

            for (int hitIndex = 0; hitIndex < hits.Length; hitIndex++)
            {
                RaycastHit hit = hits[hitIndex];
                EssencePickupView pickup = hit.collider.GetComponent<EssencePickupView>();

                if (pickup == null)
                    continue;

                if (pickup.CanAcceptHover == false)
                    continue;

                if (hit.distance >= closestHitDistance)
                    continue;

                closestHitDistance = hit.distance;
                closestPickup = pickup;
            }

            return closestPickup;
        }

        private void CollectPickup(int pickupIndex)
        {
            EssencePickupView pickup = _activePickups[pickupIndex];
            _activePickups.RemoveAt(pickupIndex);

            int collectedAmount = Mathf.FloorToInt(pickup.Amount * _essenceConfig.TowerEatFraction);
            _runEssenceService.Add(collectedAmount);

            Object.Destroy(pickup.gameObject);
        }

        private void ClearActivePickups()
        {
            for (int pickupIndex = 0; pickupIndex < _activePickups.Count; pickupIndex++)
            {
                if (_activePickups[pickupIndex] != null)
                    Object.Destroy(_activePickups[pickupIndex].gameObject);
            }

            _activePickups.Clear();
        }

        private Vector3 GetTowerPosition()
        {
            Entity mainHero = _mainHeroHolderService.MainHero;

            if (mainHero != null && mainHero.TryGetTransform(out Transform towerTransform))
                return towerTransform.position;

            return Vector3.zero;
        }

        private static float GetFlatDistance(Vector3 firstPoint, Vector3 secondPoint)
        {
            firstPoint.y = 0f;
            secondPoint.y = 0f;
            return Vector3.Distance(firstPoint, secondPoint);
        }
    }
}
