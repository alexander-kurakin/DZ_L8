using System;
using System.Collections.Generic;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Stages;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Gameplay.Features.Ability;
using Assets._Project.Develop.Runtime.Gameplay.Features.EssenceFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
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

        private readonly List<Entity> _enemiesInSector = new();

        private Entity _entity;
        private Entity _mainHero;
        private ReactiveVariable<float> _cooldownFill;
        private ReactiveEvent<Vector3> _dealAreaImpactDamageRequest;
        private IDisposable _requestDisposable;
        private float _cooldownRemaining;

        public ExplodeAtPointSystem(
            SectorMembershipService sectorMembershipService,
            SectorEnemyQueryService sectorEnemyQueryService,
            SectorRegistryService sectorRegistryService,
            LmbFlavorToastService lmbFlavorToastService,
            ExplodeAtPointAbilityConfig config)
        {
            _sectorMembershipService = sectorMembershipService;
            _sectorEnemyQueryService = sectorEnemyQueryService;
            _sectorRegistryService = sectorRegistryService;
            _lmbFlavorToastService = lmbFlavorToastService;
            _config = config;
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
        }

        private void OnAbilityUse(Vector3 usePoint)
        {
            if (_cooldownRemaining > 0f)
                return;

            if (_sectorRegistryService.IsInitialized == false)
                return;

            SectorId sectorId = _sectorMembershipService.ResolveSectorAtClick(usePoint);
            _sectorEnemyQueryService.CollectEnemiesInSector(sectorId, _enemiesInSector);

            bool showTankToast = false;
            bool showDragonToast = false;

            for (int index = 0; index < _enemiesInSector.Count; index++)
            {
                Entity enemy = _enemiesInSector[index];

                if (enemy.TryGetEnemyWavePreviewType(out WaveEnemyPreviewType previewType) == false)
                    continue;

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

            _cooldownRemaining = _config.CooldownSeconds;
            SyncCooldownFill();

            _dealAreaImpactDamageRequest?.Invoke(usePoint);
        }

        private void ApplyCatDamage(Entity enemy)
        {
            if (enemy.TryGetMaxHealth(out ReactiveVariable<float> maxHealth) == false)
                return;

            float damage = maxHealth.Value * _config.CatMaxHealthDamageFraction;

            if (damage <= 0f)
                return;

            EntitiesHelper.TryTakeDamageFrom(_entity, enemy, damage);
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
        }
    }
}
