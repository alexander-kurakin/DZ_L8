using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.Ability;
using Assets._Project.Develop.Runtime.Gameplay.Features.GameplayStateBridge;
using Assets._Project.Develop.Runtime.Gameplay.Features.MainHero;
using Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature;
using Assets._Project.Develop.Runtime.Utilities.StateMachineCore;

namespace Assets._Project.Develop.Runtime.Gameplay.States
{
    public class StageProcessState : State, IUpdatableState
    {
        private readonly StageProviderService _stageProviderService;
        private readonly MainHeroHolderService _mainHeroHolderService;
        private Entity _mainHero;

        public StageProcessState(
            StageProviderService stageProviderService,
            MainHeroHolderService mainHeroHolderService)
        {
            _stageProviderService = stageProviderService;
            _mainHeroHolderService = mainHeroHolderService;
        }

        public override void Enter()
        {
            base.Enter();
            
            _stageProviderService.SwitchToNext();
            _stageProviderService.StartCurrent();
            
            _mainHero = _mainHeroHolderService.MainHero;
         
            _mainHero.GameplayPhase.Value = GameplayStates.StageProcess;
            _mainHeroHolderService.TowerWalker.GameplayPhase.Value = _mainHeroHolderService.MainHero.GameplayPhase.Value;
            
            _mainHero.AbilityUserActiveAbility.Value = AbilityType.ExplodeAtPoint;
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
