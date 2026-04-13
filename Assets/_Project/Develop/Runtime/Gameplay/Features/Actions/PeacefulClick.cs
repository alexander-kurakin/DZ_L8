using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Meta.Features.Wallet;
using Assets._Project.Develop.Runtime.Utilities.ConfigsManagment;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.Actions
{
    public class PeacefulClick
    {
        private readonly WalletService _walletService;
        private readonly EntitiesFactory  _entitiesFactory;
        private readonly MineConfig _mineConfig;

        public PeacefulClick(
            WalletService walletService,
            EntitiesFactory entitiesFactory,
            ConfigsProviderService configsProviderService)
        {
            _walletService = walletService;
            _entitiesFactory = entitiesFactory;
            _mineConfig = configsProviderService.GetConfig<MineConfig>();    
        }

        public void TryPerformClick(RaycastHit raycastHit)
        {
            if (_walletService.Enough(CurrencyTypes.Gold, _mineConfig.MineCostInGold)) 
            {
                _walletService.Spend(CurrencyTypes.Gold, _mineConfig.MineCostInGold);
                _entitiesFactory.CreateMine(raycastHit.point, _mineConfig);
            }
        }
    }
}