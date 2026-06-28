using System;
using _Project.Develop.Runtime.Gameplay.Features.PlantableObjects;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Gameplay.Features.Ability;
using Assets._Project.Develop.Runtime.Gameplay.Features.PlantPlacementFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.SpellcoreProgressionFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature;
using Assets._Project.Develop.Runtime.Meta.Features.Wallet;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.AbilitySystems
{
    public class PlantToxicAreaSystem : IInitializableSystem, IDisposableSystem
    {
        private readonly WalletService _walletService;
        private readonly PlantableObjectsFactory _plantableObjectsFactory;
        private readonly PurchasableEntityConfig _purchasableEntityConfig;
        private readonly StageProviderService _stageProviderService;
        private readonly SpellcoreProgressionService _spellcoreProgressionService;
        private readonly PlantPlacementService _plantPlacementService;
        
        private Entity _parent;
        private IDisposable _requestDisposable;

        public PlantToxicAreaSystem(
            WalletService walletService,
            PlantableObjectsFactory plantableObjectsFactory,
            PurchasableEntityConfig purchasableEntityConfig,
            StageProviderService stageProviderService,
            SpellcoreProgressionService spellcoreProgressionService,
            PlantPlacementService plantPlacementService)
        {
            _walletService = walletService;
            _plantableObjectsFactory = plantableObjectsFactory;
            _purchasableEntityConfig = purchasableEntityConfig;
            _stageProviderService = stageProviderService;
            _spellcoreProgressionService = spellcoreProgressionService;
            _plantPlacementService = plantPlacementService;
        }

        public void OnInit(Entity entity)
        {
            _parent = entity;
            _requestDisposable = _parent.AbilityUseRequest.Subscribe(OnAbilityUse);
        }

        private void OnAbilityUse(Vector3 usePoint)
        {
            if (_spellcoreProgressionService.IsAbilityUnlocked(AbilityType.PlantToxicArea) == false)
                return;

            if (_plantPlacementService.TryResolvePlacement(
                    usePoint,
                    AbilityType.PlantToxicArea,
                    out Vector3 plantPosition,
                    out SectorId sectorId) == false)
                return;

            if (_walletService.Enough(CurrencyTypes.Diamond, _purchasableEntityConfig.CostInDiamonds)) 
            {
                _walletService.Spend(CurrencyTypes.Diamond, _purchasableEntityConfig.CostInDiamonds);
                Entity plantEntity = _plantableObjectsFactory.Create(plantPosition, _purchasableEntityConfig);
                _plantPlacementService.RegisterPlantedEntity(plantEntity, sectorId);
                _stageProviderService.AddTemporaryEntity(plantEntity);
            }            
        }

        public void OnDispose()
        {
            _requestDisposable?.Dispose();
        }
    }
}
