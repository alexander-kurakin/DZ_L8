using Assets._Project.Develop.Runtime.Gameplay.Features.JuiceFeature;
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
using _Project.Develop.Runtime.Gameplay.Features.Input;
using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.EssenceFeature
{
    public class EssenceFeatureService : IDisposable
    {
        private readonly EssenceConfig _essenceConfig;
        private readonly RunEssenceService _runEssenceService;
        private readonly EntitiesLifeContext _entitiesLifeContext;
        private readonly MainHeroHolderService _mainHeroHolderService;
        private readonly IMouseInputService _mouseInputService;
        private readonly MouseRaycastService _mouseRaycastService;
        private readonly RaycastConfig _raycastConfig;

        private readonly List<EssencePickupView> _activePickups = new();
        private readonly Dictionary<Entity, IDisposable> _enemyDeathSubscriptions = new();
        private readonly HashSet<int> _bailoutGrantedForCompletedWaves = new();

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
            _bailoutGrantedForCompletedWaves.Clear();
            _runEssenceService.InitializeForRun(_essenceConfig);
        }

        public void ClearForNewRun()
        {
            ClearActivePickups();
            _bailoutGrantedForCompletedWaves.Clear();
            _runEssenceService.ClearForNewRun();
        }

        public void TryGrantBailoutOnPreparation(Entity tower, int completedWaves)
        {
            if (_essenceConfig.BailoutEssenceAmount <= 0)
                return;

            if (_bailoutGrantedForCompletedWaves.Contains(completedWaves))
                return;

            if (tower == null)
                return;

            if (tower.TryGetCurrentHealth(out ReactiveVariable<float> currentHealth) == false)
                return;

            if (tower.TryGetMaxHealth(out ReactiveVariable<float> maxHealth) == false)
                return;

            if (maxHealth.Value <= 0f)
                return;

            float healthFraction = currentHealth.Value / maxHealth.Value;

            if (healthFraction >= _essenceConfig.BailoutTowerHealthFraction)
                return;

            _runEssenceService.Add(_essenceConfig.BailoutEssenceAmount);
            _bailoutGrantedForCompletedWaves.Add(completedWaves);
        }

        public void Update(float deltaTime)
        {
            if (_activePickups.Count == 0)
                return;

            Vector3 towerCollectPosition = GetTowerCollectPosition();
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
                    pickup.MoveTowards(towerCollectPosition, _essenceConfig.VacuumMoveSpeed, deltaTime);

                    float collectDistance = GetFlatDistance(pickup.transform.position, towerCollectPosition);

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

            if (_essenceConfig.PickupGlowPrefab == null)
            {
                Debug.LogError("EssenceConfig.PickupGlowPrefab is not assigned.");
                return;
            }

            int dropAmount = _essenceConfig.GetDropAmountFor(previewType);

            if (dropAmount <= 0)
                return;

            Vector3 spawnPosition = enemyTransform.position;
            Vector3 pickupWorldPosition = spawnPosition;
            pickupWorldPosition.y += _essenceConfig.PickupFloorOffset;

            if (_essenceConfig.PickupDropPrefab != null)
            {
                GameplayVfxUtility.SpawnTransientAt(
                    _essenceConfig.PickupDropPrefab,
                    pickupWorldPosition,
                    Quaternion.identity,
                    uniformScale: _essenceConfig.PickupDropVfxScale);
            }

            GameObject pickupRoot = new GameObject("EssencePickup");
            GameObject glowInstance = Object.Instantiate(_essenceConfig.PickupGlowPrefab, pickupRoot.transform);
            glowInstance.transform.localPosition = Vector3.zero;
            glowInstance.transform.localRotation = Quaternion.identity;
            glowInstance.transform.localScale = Vector3.one;

            EssencePickupView pickup = pickupRoot.AddComponent<EssencePickupView>();
            pickup.Initialize(
                dropAmount,
                spawnPosition,
                _essenceConfig,
                _essenceConfig.PickupVacuumTrailPrefab);
            _activePickups.Add(pickup);
        }

        private EssencePickupView TryGetHoveredPickup()
        {
            Vector2 pointerScreenPosition = _mouseInputService.PointerScreenPosition;
            Ray ray = Camera.main.ScreenPointToRay(pointerScreenPosition);
            RaycastHit[] hits = Physics.RaycastAll(
                ray,
                _raycastConfig.MouseRaycastDistance,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Collide);

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

            PlayTowerCollectVfx();
            Object.Destroy(pickup.gameObject);
        }

        private void PlayTowerCollectVfx()
        {
            if (_essenceConfig.TowerCollectPrefab == null)
                return;

            Entity mainHero = _mainHeroHolderService.MainHero;

            if (mainHero == null || mainHero.TryGetTransform(out Transform towerTransform) == false)
                return;

            Vector3 spawnPosition = towerTransform.position;
            spawnPosition.y += _essenceConfig.TowerCollectHeightOffset;

            GameplayVfxUtility.SpawnTransientAt(
                _essenceConfig.TowerCollectPrefab,
                spawnPosition,
                Quaternion.identity,
                towerTransform,
                _essenceConfig.TowerCollectVfxScale);
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

        private Vector3 GetTowerCollectPosition()
        {
            Entity mainHero = _mainHeroHolderService.MainHero;

            if (mainHero != null && mainHero.TryGetTransform(out Transform towerTransform))
            {
                Vector3 towerCollectPosition = towerTransform.position;
                towerCollectPosition.y += _essenceConfig.TowerCollectHeightOffset;
                return towerCollectPosition;
            }

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
