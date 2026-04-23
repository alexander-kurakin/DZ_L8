using Assets._Project.Develop.Runtime.Configs.Meta.Stats;
using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;
using Assets._Project.Develop.Runtime.Utilities.CoroutinesManagment;
using Assets._Project.Develop.Runtime.Utilities.DataManagment.DataProviders;
using Assets._Project.Develop.Runtime.Utilities.SceneManagment;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.States
{
    public class DefeatState : EndGameState
    {
        public DefeatState(
            IInputService inputService,
            PlayerDataProvider playerDataProvider,
            SceneSwitcherService sceneSwitcherService,
            ICoroutinesPerformer coroutinesPerformer,
            StatsService statsService ) : base(inputService, playerDataProvider, sceneSwitcherService, coroutinesPerformer, statsService)
        {
        }
        
        protected override void OnEndGameStateEntered()
        {
            Debug.Log("ПОРАЖЕНИЕ! Нажмите Q чтобы вернуться в главное меню");
        }

        protected override void RecordResults()
        {
            Stats.RecordLoss();
        }
    }
}
