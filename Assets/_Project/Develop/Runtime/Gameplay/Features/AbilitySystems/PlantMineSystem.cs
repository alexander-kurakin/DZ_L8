using System;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Meta.Features.Wallet;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Ability
{
    public class PlantMineSystem : IInitializableSystem, IDisposableSystem
    {
        private readonly WalletService _walletService;
        private readonly EntitiesFactory _entitiesFactory;
        private readonly MineConfig _mineConfig;

        private Entity _entity;
        private IDisposable _requestDisposable;

        public PlantMineSystem(
            WalletService walletService,
            EntitiesFactory entitiesFactory,
            MineConfig mineConfig)
        {
            _walletService = walletService;
            _entitiesFactory = entitiesFactory;
            _mineConfig = mineConfig;
        }

        public void OnInit(Entity entity)
        {
            _entity = entity;
            _requestDisposable = _entity.AbilityUseRequest.Subscribe(OnAbilityUse);
        }

        private void OnAbilityUse(Vector3 usePoint)
        {
            if (_walletService.Enough(CurrencyTypes.Gold, _mineConfig.MineCostInGold)) 
            {
                _walletService.Spend(CurrencyTypes.Gold, _mineConfig.MineCostInGold);
                _entitiesFactory.CreateMine(usePoint, _mineConfig);
            }
        }

        public void OnDispose()
        {
            _requestDisposable?.Dispose();
        }
    }
}