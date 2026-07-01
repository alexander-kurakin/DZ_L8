namespace Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature
{
    public class SurvivalFlowService
    {
        public const int SURVIVAL_MILESTONE_BONUS_GOLD = 2000;

        private readonly StageProviderService _stageProviderService;

        private bool _survivalOfferPending;
        private bool _survivalMilestonePending;
        private bool _campaignCompletionGoldGranted;
        private bool _milestoneGoldGranted;

        public SurvivalFlowService(StageProviderService stageProviderService)
        {
            _stageProviderService = stageProviderService;
        }

        public bool IsSurvivalOfferPending => _survivalOfferPending;

        public bool IsSurvivalMilestonePending => _survivalMilestonePending;

        public bool IsSurvivalModeEntered => _stageProviderService.IsSurvivalModeActive;

        public bool ShouldBlockAutomaticWin =>
            _survivalOfferPending || _survivalMilestonePending;

        public void OnNormalCampaignCompleted()
        {
            _survivalOfferPending = true;
        }

        public void OnSurvivalMilestoneReached()
        {
            if (_stageProviderService.IsSurvivalModeActive == false)
                return;

            _survivalMilestonePending = true;
            _milestoneGoldGranted = false;
        }

        public void EnterSurvivalMode()
        {
            _survivalOfferPending = false;
            _stageProviderService.ActivateSurvivalMode();
        }

        public void ClearSurvivalMilestonePending()
        {
            _survivalMilestonePending = false;
        }

        public bool TryConsumeCampaignCompletionGoldGrant()
        {
            if (_survivalOfferPending == false || _campaignCompletionGoldGranted)
                return false;

            _campaignCompletionGoldGranted = true;
            return true;
        }

        public bool TryConsumeMilestoneGoldGrant()
        {
            if (_survivalMilestonePending == false || _milestoneGoldGranted)
                return false;

            _milestoneGoldGranted = true;
            return true;
        }

        public bool ShouldShowSurvivalPlusSuffix(int displayWave, int normalStagesCount)
        {
            if (_stageProviderService.IsSurvivalModeActive == false)
                return false;

            return displayWave >= normalStagesCount;
        }
    }
}
