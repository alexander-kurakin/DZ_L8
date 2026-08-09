using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.GameplayStateBridge;
using Assets._Project.Develop.Runtime.Gameplay.Features.MainHero;
using Assets._Project.Develop.Runtime.Gameplay.Features.SpellcoreProgressionFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature;
using _Project.Develop.Runtime.Gameplay.Features.LeftClickAbilityPreview;
using Assets._Project.Develop.Runtime.Utilities.StateMachineCore;

namespace Assets._Project.Develop.Runtime.Gameplay.States
{
    public class StageProcessState : State, IUpdatableState
    {
        private readonly StageProviderService _stageProviderService;
        private readonly MainHeroHolderService _mainHeroHolderService;
        private readonly SpellcoreProgressionService _spellcoreProgressionService;
        private readonly LmbFrostProjectileService _lmbFrostProjectileService;
        private Entity _mainHero;

        public StageProcessState(
            StageProviderService stageProviderService,
            MainHeroHolderService mainHeroHolderService,
            SpellcoreProgressionService spellcoreProgressionService,
            LmbFrostProjectileService lmbFrostProjectileService)
        {
            _stageProviderService = stageProviderService;
            _mainHeroHolderService = mainHeroHolderService;
            _spellcoreProgressionService = spellcoreProgressionService;
            _lmbFrostProjectileService = lmbFrostProjectileService;
        }

        public override void Enter()
        {
            base.Enter();
            
            _stageProviderService.SwitchToNext();
            _stageProviderService.StartCurrent();

            _spellcoreProgressionService.OnCombatWaveEntered(_stageProviderService.CurrentStageNumber.Value);
            
            _mainHero = _mainHeroHolderService.MainHero;

            _lmbFrostProjectileService.ClearQueuedProjectileLaunch();
         
            _mainHero.GameplayPhase.Value = GameplayStates.StageProcess;
            _mainHeroHolderService.TowerWalker.GameplayPhase.Value = _mainHeroHolderService.MainHero.GameplayPhase.Value;
            _mainHeroHolderService.TowerBrother.GameplayPhase.Value = _mainHeroHolderService.MainHero.GameplayPhase.Value;

            _mainHero.AbilityUserActiveAbility.Value = _mainHero.AbilityUserPlantAbilityPreference.Value;
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
