using System;
using System.Collections.Generic;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Stages;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.GameplayStateBridge;
using Assets._Project.Develop.Runtime.Gameplay.Features.TakeDamage;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Gameplay.Features.Ability;
using Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using _Project.Develop.Runtime.Gameplay.Features.ExplosionAbilityPreview;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.AbilitySystems
{
    public class ExplodeAtPointSystem : IInitializableSystem, IUpdatableSystem, IDisposableSystem
    {
        private readonly SectorMembershipService _sectorMembershipService;
        private readonly SectorEnemyQueryService _sectorEnemyQueryService;
        private readonly SectorRegistryService _sectorRegistryService;
        private readonly LmbFlavorToastService _lmbFlavorToastService;
        private readonly ExplodeAtPointAbilityConfig _config;
        private readonly LmbFrostProjectileService _lmbFrostProjectileService;

        private readonly List<Entity> _enemiesInSector = new();

        private Entity _entity;
        private Entity _mainHero;
        private ReactiveVariable<float> _cooldownFill;
        private ReactiveVariable<GameplayStates> _gameplayPhase;
        private ReactiveEvent<Vector3> _dealAreaImpactDamageRequest;
        private IDisposable _requestDisposable;
        private float _cooldownRemaining;

        public ExplodeAtPointSystem(
            SectorMembershipService sectorMembershipService,
            SectorEnemyQueryService sectorEnemyQueryService,
            SectorRegistryService sectorRegistryService,
            LmbFlavorToastService lmbFlavorToastService,
            ExplodeAtPointAbilityConfig config,
            LmbFrostProjectileService lmbFrostProjectileService)
        {
            _sectorMembershipService = sectorMembershipService;
            _sectorEnemyQueryService = sectorEnemyQueryService;
            _sectorRegistryService = sectorRegistryService;
            _lmbFlavorToastService = lmbFlavorToastService;
            _config = config;
            _lmbFrostProjectileService = lmbFrostProjectileService;
        }

        public void OnInit(Entity entity)
        {
            _entity = entity;
            _dealAreaImpactDamageRequest = entity.DealAreaImpactDamageRequest;
            _requestDisposable = _entity.AbilityUseRequest.Subscribe(OnAbilityUse);

            if (_entity.TryGetAbilityOwner(out ReactiveVariable<Entity> abilityOwner))
                _mainHero = abilityOwner.Value;

            if (_mainHero != null && _mainHero.TryGetExplosionPreviewCooldownFill(out _cooldownFill))
                _cooldownFill.Value = 0f;

            if (_mainHero != null)
                _gameplayPhase = _mainHero.GameplayPhase;

            _lmbFrostProjectileService.Configure(_config);
            _lmbFrostProjectileService.RegisterImpactHandler(OnProjectileImpact);
        }

        private void OnAbilityUse(Vector3 usePoint)
        {
            if (IsCombatPhase() == false)
                return;

            if (_cooldownRemaining > 0f)
                return;

            if (_sectorRegistryService.IsInitialized == false)
                return;

            Vector3 impactPoint = GetClickImpactPoint(usePoint);

            _cooldownRemaining = _config.CooldownSeconds;
            SyncCooldownFill();

            _lmbFrostProjectileService.ShowTargetOrbs(
                _config.FrostTargetOrbsPrefab,
                impactPoint,
                _config.FrostTargetOrbsScale);
            _lmbFrostProjectileService.QueueProjectileLaunch(impactPoint);
        }

        private bool IsCombatPhase()
        {
            if (_gameplayPhase == null)
                return false;

            return _gameplayPhase.Value == GameplayStates.StageProcess;
        }

        private Vector3 GetClickImpactPoint(Vector3 clickWorldPoint)
        {
            float impactPlaneY = _sectorRegistryService.Center.y + _config.ImpactGroundYOffset;
            return new Vector3(clickWorldPoint.x, impactPlaneY, clickWorldPoint.z);
        }

        private void OnProjectileImpact(Vector3 impactPoint)
        {
            SectorId sectorId = _sectorMembershipService.ResolveFromWorldPosition(impactPoint);
            _sectorEnemyQueryService.CollectEnemiesInSector(sectorId, _enemiesInSector);

            SectorId clickPositionSector = _sectorMembershipService.ResolveFromWorldPosition(impactPoint);

            if (clickPositionSector != sectorId)
                _sectorEnemyQueryService.AppendEnemiesInSector(clickPositionSector, _enemiesInSector);

            _sectorEnemyQueryService.AppendEnemiesNearWorldPosition(impactPoint, _config.NearbyEnemyRadius, _enemiesInSector);

            bool showTankToast = false;
            bool showDragonToast = false;

            for (int index = 0; index < _enemiesInSector.Count; index++)
            {
                Entity enemy = _enemiesInSector[index];

                if (IsEnemyInLmbDamageBelt(enemy) == false)
                    continue;

                if (enemy.TryGetEnemyWavePreviewType(out WaveEnemyPreviewType previewType) == false)
                {
                    ApplyCatDamage(enemy);
                    continue;
                }

                switch (previewType)
                {
                    case WaveEnemyPreviewType.Cat:
                        ApplyCatDamage(enemy);
                        break;

                    case WaveEnemyPreviewType.Tank:
                        showTankToast = true;
                        break;

                    case WaveEnemyPreviewType.Dragon:
                        showDragonToast = true;
                        break;
                }
            }

            if (showTankToast)
                _lmbFlavorToastService.Show(LmbFlavorToastType.TankArmor);

            if (showDragonToast)
                _lmbFlavorToastService.Show(LmbFlavorToastType.DragonMagicalDefense);

            _dealAreaImpactDamageRequest?.Invoke(impactPoint);
        }

        private bool IsEnemyInLmbDamageBelt(Entity enemy)
        {
            if (enemy.TryGetTransform(out Transform enemyTransform) == false)
                return false;

            SectorId enemySector = _sectorMembershipService.ResolveFromWorldPosition(enemyTransform.position);

            return enemySector.Belt != SectorBelt.Spawn;
        }

        private void ApplyCatDamage(Entity enemy)
        {
            if (enemy.TryGetMaxHealth(out ReactiveVariable<float> maxHealth) == false)
                return;

            float damage = maxHealth.Value * _config.CatMaxHealthDamageFraction;

            if (damage <= 0f)
                return;

            EntitiesHelper.TryTakeDamageFrom(_entity, enemy, damage, TakeDamageVisualKind.SectorAbility);
        }

        public void OnUpdate(float deltaTime)
        {
            if (_cooldownRemaining > 0f)
            {
                _cooldownRemaining -= deltaTime;

                if (_cooldownRemaining < 0f)
                    _cooldownRemaining = 0f;
            }

            SyncCooldownFill();
        }

        private void SyncCooldownFill()
        {
            if (_cooldownFill == null)
                return;

            if (_config.CooldownSeconds <= 0f)
            {
                _cooldownFill.Value = 0f;
                return;
            }

            _cooldownFill.Value = _cooldownRemaining / _config.CooldownSeconds;
        }

        public void OnDispose()
        {
            _requestDisposable?.Dispose();
            _lmbFrostProjectileService.ClearImpactHandler();
        }
    }
}
