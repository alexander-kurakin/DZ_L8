using System;
using _Project.Develop.Runtime.Gameplay.Features.PlantableObjects;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
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
        
        private Entity _parent;
        private Entity _child;
        private IDisposable _requestDisposable;

        public PlantToxicAreaSystem(
            WalletService walletService,
            PlantableObjectsFactory plantableObjectsFactory,
            PurchasableEntityConfig purchasableEntityConfig,
            StageProviderService stageProviderService)
        {
            _walletService = walletService;
            _plantableObjectsFactory = plantableObjectsFactory;
            _purchasableEntityConfig = purchasableEntityConfig;
            _stageProviderService = stageProviderService;
        }

        public void OnInit(Entity entity)
        {
            _parent = entity;
            _requestDisposable = _parent.AbilityUseRequest.Subscribe(OnAbilityUse);
        }

        private void OnAbilityUse(Vector3 usePoint)
        {
            if (_walletService.Enough(CurrencyTypes.Gold, _purchasableEntityConfig.CostInGold)) 
            {
                _walletService.Spend(CurrencyTypes.Gold, _purchasableEntityConfig.CostInGold);
                _child = _plantableObjectsFactory.Create(usePoint, _purchasableEntityConfig);
                _stageProviderService.AddTemporaryEntity(_child);
            }            
        }

        public void OnDispose()
        {
            _requestDisposable?.Dispose();
        }
    }
}