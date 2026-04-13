using _Project.Develop.Runtime.Gameplay.Features.Actions;
using Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature;
using Assets._Project.Develop.Runtime.Utilities.StateMachineCore;

namespace Assets._Project.Develop.Runtime.Gameplay.States
{
    public class StageProcessState : State, IUpdatableState
    {
        private readonly StageProviderService _stageProviderService;
        private readonly GameplayActionSetService _actionSetService;

        public StageProcessState(
            StageProviderService stageProviderService, 
            GameplayActionSetService actionSetService)
        {
            _stageProviderService = stageProviderService;
            _actionSetService = actionSetService;
        }

        public override void Enter()
        {
            base.Enter();
            
            _actionSetService.SetActionSet(ActionSet.Combat);

            _stageProviderService.SwitchToNext();
            _stageProviderService.StartCurrent();
        }

        public void Update(float deltaTime)
        {
            _stageProviderService.UpdateCurrent(deltaTime);
        }

        public override void Exit()
        {
            base.Exit();

            _stageProviderService.CleanupCurrent();
        }
    }
}
