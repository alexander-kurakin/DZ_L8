using System;
using _Project.Develop.Runtime.Gameplay.Features.PlantableObjects;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Gameplay.Features.Ability;
using Assets._Project.Develop.Runtime.Gameplay.Features.SpellcoreProgressionFeature;
using Assets._Project.Develop.Runtime.Meta.Features.Wallet;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.AbilitySystems
{
    public class PlantTurretSystem : IInitializableSystem, IDisposableSystem
    {
        private readonly WalletService _walletService;
        private readonly PlantableObjectsFactory _plantableObjectsFactory;
        private readonly PurchasableEntityConfig _purchasableEntityConfig;
        private readonly SpellcoreProgressionService _spellcoreProgressionService;
        
        private Entity _entity;
        private IDisposable _requestDisposable;

        public PlantTurretSystem(
            WalletService walletService,
            PlantableObjectsFactory plantableObjectsFactory,
            PurchasableEntityConfig purchasableEntityConfig,
            SpellcoreProgressionService spellcoreProgressionService)
        {
            _walletService = walletService;
            _plantableObjectsFactory = plantableObjectsFactory;
            _purchasableEntityConfig = purchasableEntityConfig;
            _spellcoreProgressionService = spellcoreProgressionService;
        }

        public void OnInit(Entity entity)
        {
            _entity = entity;
            _requestDisposable = _entity.AbilityUseRequest.Subscribe(OnAbilityUse);
        }

        private void OnAbilityUse(Vector3 usePoint)
        {
            if (_spellcoreProgressionService.IsAbilityUnlocked(AbilityType.PlantTurret) == false)
                return;

            if (_walletService.Enough(CurrencyTypes.Diamond, _purchasableEntityConfig.CostInDiamonds)) 
            {
                _walletService.Spend(CurrencyTypes.Diamond, _purchasableEntityConfig.CostInDiamonds);
                _plantableObjectsFactory.Create(usePoint, _purchasableEntityConfig);
            }            
        }

        public void OnDispose()
        {
            _requestDisposable?.Dispose();
        }
    }
}