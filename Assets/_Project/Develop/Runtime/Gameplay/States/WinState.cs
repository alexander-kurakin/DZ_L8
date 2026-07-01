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
        private readonly PersistedGoldRewardService _goldRewardService;
        private readonly GameplayPopupService _popupService;
        
        private readonly int _rewardGold;
        
        public WinState(
            IInputService inputService,
            PlayerDataProvider playerDataProvider,
            SceneSwitcherService sceneSwitcherService,
            ICoroutinesPerformer coroutinesPerformer,
            StatsService statsService,
            PersistedGoldRewardService goldRewardService,
            GameplayPopupService popupService,
            IMouseInputService mouseInputService,
            int rewardGold) : base(inputService, playerDataProvider, sceneSwitcherService, coroutinesPerformer, statsService, mouseInputService)
        {
            _goldRewardService = goldRewardService;
            _rewardGold = rewardGold;
            _popupService = popupService;
        }

        protected override void RecordResults()
        {
            Stats.RecordWin();
            
            RewardsData rewardsData = new RewardsData
            {
                RewardGold = _goldRewardService.AddGold(_rewardGold)
            };
            
            _popupService.OpenWinPopup(rewardsData);
        }
    }

    public class RewardsData
    {
        public int RewardGold;
    }
}
