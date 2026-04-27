using _Project.Develop.Runtime.UI.Gameplay;
using Assets._Project.Develop.Runtime.Configs.Meta.Stats;
using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;
using Assets._Project.Develop.Runtime.Meta.Features.Wallet;
using Assets._Project.Develop.Runtime.Utilities.CoroutinesManagment;
using Assets._Project.Develop.Runtime.Utilities.DataManagment.DataProviders;
using Assets._Project.Develop.Runtime.Utilities.SceneManagment;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.States
{
    public class WinState : EndGameState
    {
        private readonly WalletService _walletService;
        private readonly int _rewardGold;
        private readonly int _rewardDiamondMin;
        private readonly int _rewardDiamondMax;
        private readonly GameplayPopupService _popupService;
        
        
        public WinState(
            IInputService inputService,
            PlayerDataProvider playerDataProvider,
            SceneSwitcherService sceneSwitcherService,
            ICoroutinesPerformer coroutinesPerformer,
            StatsService statsService,
            WalletService walletService,
            GameplayPopupService  popupService,
            int rewardGold,
            int rewardDiamondMin,
            int rewardDiamondMax) : base(inputService, playerDataProvider, sceneSwitcherService, coroutinesPerformer, statsService)
        {
            _walletService = walletService;
            _rewardGold = rewardGold;
            _popupService = popupService;
            _rewardDiamondMin = rewardDiamondMin;
            _rewardDiamondMax = rewardDiamondMax;
        }

        protected override void OnEndGameStateEntered()
        {
            _popupService.OpenWinPopup();
        }

        protected override void RecordResults()
        {
            Stats.RecordWin();
            
            if (_rewardGold > 0)
                _walletService.Add(CurrencyTypes.Gold, _rewardGold);
            
            _walletService.Add(CurrencyTypes.Diamond, Random.Range(_rewardDiamondMin, _rewardDiamondMax));            
        }

    }
}
