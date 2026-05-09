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
    public class PlantMineSystem : IInitializableSystem, IDisposableSystem
    {
        private readonly WalletService _walletService;
        private readonly PlantableObjectsFactory _plantableObjectsFactory;
        private readonly PurchasableEntityConfig _purchasableEntityConfig;
        
        private Entity _entity;
        private IDisposable _requestDisposable;

        public PlantMineSystem(
            WalletService walletService,
            PlantableObjectsFactory plantableObjectsFactory,
            PurchasableEntityConfig purchasableEntityConfig)
        {
            _walletService = walletService;
            _plantableObjectsFactory = plantableObjectsFactory;
            _purchasableEntityConfig = purchasableEntityConfig;
        }

        public void OnInit(Entity entity)
        {
            _entity = entity;
            _requestDisposable = _entity.AbilityUseRequest.Subscribe(OnAbilityUse);
        }

        private void OnAbilityUse(Vector3 usePoint)
        {
            if (_walletService.Enough(CurrencyTypes.Gold, _purchasableEntityConfig.CostInGold)) 
            {
                _walletService.Spend(CurrencyTypes.Gold, _purchasableEntityConfig.CostInGold);
                _plantableObjectsFactory.Create(usePoint, _purchasableEntityConfig);
            }            
        }

        public void OnDispose()
        {
            _requestDisposable?.Dispose();
        }
    }
}