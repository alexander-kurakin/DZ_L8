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
using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.EssenceFeature
{
    public class EssenceFeatureService : IDisposable
    {
        private readonly EssenceConfig _essenceConfig;
        private readonly RunEssenceService _runEssenceService;
        private readonly EntitiesFactory _entitiesFactory;
        private readonly EntitiesLifeContext _entitiesLifeContext;
        private readonly CollidersRegistryService _collidersRegistryService;
        private readonly MainHeroHolderService _mainHeroHolderService;
        private readonly IMouseInputService _mouseInputService;
        private readonly RaycastConfig _raycastConfig;

        private readonly List<Entity> _activePickups = new();
        private readonly Dictionary<Entity, IDisposable> _enemyDeathSubscriptions = new();
        private readonly HashSet<int> _bailoutGrantedForCompletedWaves = new();

        public EssenceFeatureService(
            ConfigsProviderService configsProviderService,
            RunEssenceService runEssenceService,
            EntitiesFactory entitiesFactory,
            EntitiesLifeContext entitiesLifeContext,
            CollidersRegistryService collidersRegistryService,
            MainHeroHolderService mainHeroHolderService,
            IMouseInputService mouseInputService)
        {
            _essenceConfig = configsProviderService.GetConfig<EssenceConfig>();
            _runEssenceService = runEssenceService;
            _entitiesFactory = entitiesFactory;
            _entitiesLifeContext = entitiesLifeContext;
            _collidersRegistryService = collidersRegistryService;
            _mainHeroHolderService = mainHeroHolderService;
            _mouseInputService = mouseInputService;
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

        public void ActivateAutoHoverForAllPickupsOnPreparation()
        {
            for (int pickupIndex = 0; pickupIndex < _activePickups.Count; pickupIndex++)
            {
                Entity pickup = _activePickups[pickupIndex];

                if (pickup == null)
                    continue;

                if (pickup.TryGetEssenceStartVacuumRequest(out ReactiveEvent startVacuumRequest) == false)
                    continue;

                startVacuumRequest.Invoke();
            }
        }

        public void Update(float deltaTime)
        {
            if (_activePickups.Count == 0)
                return;

            Entity hoveredPickup = TryGetHoveredPickup();

            if (hoveredPickup == null)
                return;

            if (hoveredPickup.TryGetEssenceIsVacuuming(out ReactiveVariable<bool> isVacuuming) == false)
                return;

            if (isVacuuming.Value)
                return;

            if (hoveredPickup.TryGetEssenceCanAcceptHover(out ReactiveVariable<bool> canAcceptHover) == false)
                return;

            if (canAcceptHover.Value == false)
                return;

            hoveredPickup.EssenceStartVacuumRequest.Invoke();
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
            _activePickups.Remove(entity);

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
            Vector3 dropVfxPosition = spawnPosition;
            dropVfxPosition.y += _essenceConfig.PickupFloorOffset;

            if (_essenceConfig.PickupDropPrefab != null)
            {
                GameplayVfxUtility.SpawnTransientAt(
                    _essenceConfig.PickupDropPrefab,
                    dropVfxPosition,
                    Quaternion.identity,
                    uniformScale: _essenceConfig.PickupDropVfxScale);
            }

            Entity pickup = _entitiesFactory.CreateEssencePickup(spawnPosition, dropAmount, _essenceConfig);
            _activePickups.Add(pickup);
        }

        private Entity TryGetHoveredPickup()
        {
            Vector2 pointerScreenPosition = _mouseInputService.PointerScreenPosition;
            Ray ray = Camera.main.ScreenPointToRay(pointerScreenPosition);
            RaycastHit[] hits = Physics.RaycastAll(
                ray,
                _raycastConfig.MouseRaycastDistance,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Collide);

            Entity closestPickup = null;
            float closestHitDistance = float.MaxValue;

            for (int hitIndex = 0; hitIndex < hits.Length; hitIndex++)
            {
                RaycastHit hit = hits[hitIndex];
                Entity hitEntity = _collidersRegistryService.GetBy(hit.collider);

                if (hitEntity == null)
                    continue;

                if (hitEntity.TryGetEssenceCanAcceptHover(out ReactiveVariable<bool> canAcceptHover) == false)
                    continue;

                if (canAcceptHover.Value == false)
                    continue;

                if (hit.distance >= closestHitDistance)
                    continue;

                closestHitDistance = hit.distance;
                closestPickup = hitEntity;
            }

            return closestPickup;
        }

        private void ClearActivePickups()
        {
            for (int pickupIndex = 0; pickupIndex < _activePickups.Count; pickupIndex++)
            {
                Entity pickup = _activePickups[pickupIndex];

                if (pickup == null)
                    continue;

                _entitiesLifeContext.Release(pickup);
            }

            _activePickups.Clear();
        }
    }
}
