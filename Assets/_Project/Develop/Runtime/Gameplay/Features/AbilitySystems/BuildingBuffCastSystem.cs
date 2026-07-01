using System;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.GameplayStateBridge;
using Assets._Project.Develop.Runtime.Gameplay.Features.JuiceFeature;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Gameplay.Features.PlantBuildingBuff;
using Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using _Project.Develop.Runtime.Gameplay.Features.LeftClickAbilityPreview;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.AbilitySystems
{
    public class BuildingBuffCastSystem : IInitializableSystem, IUpdatableSystem, IDisposableSystem
    {
        private readonly SectorRegistryService _sectorRegistryService;
        private readonly BuildingBuffCastAbilityConfig _config;
        private readonly LmbFrostProjectileService _lmbFrostProjectileService;
        private readonly GameplayJuiceService _gameplayJuiceService;
        private readonly PlantBuildingBuffService _plantBuildingBuffService;

        private Entity _entity;
        private Entity _mainHero;
        private ReactiveVariable<float> _cooldownFill;
        private ReactiveVariable<GameplayStates> _gameplayPhase;
        private IDisposable _requestDisposable;
        private float _cooldownRemaining;

        public BuildingBuffCastSystem(
            SectorRegistryService sectorRegistryService,
            BuildingBuffCastAbilityConfig config,
            LmbFrostProjectileService lmbFrostProjectileService,
            GameplayJuiceService gameplayJuiceService,
            PlantBuildingBuffService plantBuildingBuffService)
        {
            _sectorRegistryService = sectorRegistryService;
            _config = config;
            _lmbFrostProjectileService = lmbFrostProjectileService;
            _gameplayJuiceService = gameplayJuiceService;
            _plantBuildingBuffService = plantBuildingBuffService;
        }

        public void OnInit(Entity entity)
        {
            _entity = entity;
            _requestDisposable = _entity.AbilityUseRequest.Subscribe(OnAbilityUse);

            if (_entity.TryGetAbilityOwner(out ReactiveVariable<Entity> abilityOwner))
                _mainHero = abilityOwner.Value;

            if (_mainHero != null && _mainHero.TryGetLeftClickPreviewCooldownFill(out _cooldownFill))
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
            _gameplayJuiceService.PlayScreenShakeSmall();
            _plantBuildingBuffService.TryApplyBuffAtWorldPosition(impactPoint, _config);
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
