using System;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature;
using Assets._Project.Develop.Runtime.Meta.Features.Wallet;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.AbilitySystems
{
    public class PlantPurchasedObjectsSystem : IInitializableSystem, IDisposableSystem
    {
        private readonly WalletService _walletService;
        private readonly EntitiesFactory _entitiesFactory;
        private readonly PurchasableEntityConfig _purchasableEntityConfig;
        private readonly StageProviderService _stageProviderService;
        
        private Entity _entity;
        private IDisposable _requestDisposable;

        public PlantPurchasedObjectsSystem(
            WalletService walletService,
            EntitiesFactory entitiesFactory,
            PurchasableEntityConfig purchasableEntityConfig,
            StageProviderService stageProviderService)
        {
            _walletService = walletService;
            _entitiesFactory = entitiesFactory;
            _purchasableEntityConfig = purchasableEntityConfig;
            _stageProviderService = stageProviderService;
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

                switch (_purchasableEntityConfig)
                {
                    case MineConfig mineConfig:
                    {
                        _entitiesFactory.CreateMine(usePoint, mineConfig);
                        break;
                    }
                    case TurretConfig turretConfig:
                    {
                        _entitiesFactory.CreateTurret(usePoint, turretConfig);
                        break;
                    }
                    case ToxicAreaConfig toxicAreaConfig:
                    {
                        Entity toxicEntity = _entitiesFactory.CreateToxicArea(usePoint, toxicAreaConfig);
                        _stageProviderService.AddToxicArea(toxicEntity);
                        break;
                    }
                    default:
                        throw new ArgumentException($"Not supported {_purchasableEntityConfig.GetType()} type config");
                } 
            }            
        }

        public void OnDispose()
        {
            _requestDisposable?.Dispose();
        }
    }
}