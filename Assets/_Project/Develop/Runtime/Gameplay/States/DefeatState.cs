using _Project.Develop.Runtime.UI.Gameplay;
using Assets._Project.Develop.Runtime.Configs.Meta.Stats;
using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;
using Assets._Project.Develop.Runtime.Utilities.CoroutinesManagment;
using Assets._Project.Develop.Runtime.Utilities.DataManagment.DataProviders;
using Assets._Project.Develop.Runtime.Utilities.SceneManagment;

namespace Assets._Project.Develop.Runtime.Gameplay.States
{
    public class DefeatState : EndGameState
    {
        private readonly GameplayPopupService _popupService;
        
        public DefeatState(
            IInputService inputService,
            PlayerDataProvider playerDataProvider,
            SceneSwitcherService sceneSwitcherService,
            ICoroutinesPerformer coroutinesPerformer,
            StatsService statsService,
            GameplayPopupService popupService) : base(inputService, playerDataProvider, sceneSwitcherService, coroutinesPerformer, statsService)
        {
            _popupService =  popupService;
        }
        
        protected override void OnEndGameStateEntered()
        {
            _popupService.OpenDefeatPopup();
        }

        protected override void RecordResults()
        {
            Stats.RecordLoss();
        }
    }
}
