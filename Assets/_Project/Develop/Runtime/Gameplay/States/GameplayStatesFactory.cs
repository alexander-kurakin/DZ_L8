using _Project.Develop.Runtime.Gameplay.Features.LeftClickAbilityPreview;
using _Project.Develop.Runtime.Gameplay.Features.Input;
using _Project.Develop.Runtime.Gameplay.Features.InputFeature;
using _Project.Develop.Runtime.UI.Gameplay;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Levels;
using Assets._Project.Develop.Runtime.Configs.Meta.Stats;
using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.MainHero;
using Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.EssenceFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.SpellcoreProgressionFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.Ability;
using Assets._Project.Develop.Runtime.Gameplay.Features.PlantBuildingBuff;
using Assets._Project.Develop.Runtime.Gameplay.Features.PlantPlacementFeature;
using Assets._Project.Develop.Runtime.Gameplay.Infrastructure;
using Assets._Project.Develop.Runtime.Infrastructure.DI;
using Assets._Project.Develop.Runtime.Meta.Features.Wallet;
using Assets._Project.Develop.Runtime.Utilities.Audio;
using Assets._Project.Develop.Runtime.Utilities.Conditions;
using Assets._Project.Develop.Runtime.Utilities.ConfigsManagment;
using Assets._Project.Develop.Runtime.Utilities.CoroutinesManagment;
using Assets._Project.Develop.Runtime.Utilities.DataManagment.DataProviders;
using Assets._Project.Develop.Runtime.Utilities.SceneManagment;

namespace Assets._Project.Develop.Runtime.Gameplay.States
{
    public class GameplayStatesFactory
    {
        private readonly DIContainer _container;

        public GameplayStatesFactory(DIContainer container)
        {
            _container = container;
       }

        public PreparationState CreatePreparationState()
        {
            ConfigsProviderService configsProviderService = _container.Resolve<ConfigsProviderService>();
            GameplayInputArgs gameplayInputArgs = _container.Resolve<GameplayInputArgs>();
            int levelGoldReward = configsProviderService
                .GetConfig<LevelsListConfig>()
                .GetBy(gameplayInputArgs.LevelNumber)
                .GoldReward;

            return new PreparationState(
                _container.Resolve<PreparationTriggerService>(),
                configsProviderService,
                _container.Resolve<MainHeroHolderService>(),
                _container.Resolve<MouseRaycastService>(),
                _container.Resolve<IMouseInputService>(),
                _container.Resolve<IBackgroundMusicService>(),
                _container.Resolve<MouseOverUIService>(),
                _container.Resolve<SpellcoreProgressionService>(),
                _container.Resolve<SectorRegistryService>(),
                _container.Resolve<LmbFrostProjectileService>(),
                _container.Resolve<EssenceFeatureService>(),
                _container.Resolve<PlantBuildingBuffService>(),
                _container.Resolve<PlantSellInputService>(),
                _container.Resolve<SurvivalFlowService>(),
                _container.Resolve<GameplayPopupService>(),
                _container.Resolve<PersistedGoldRewardService>(),
                _container.Resolve<SceneSwitcherService>(),
                _container.Resolve<ICoroutinesPerformer>(),
                _container.Resolve<SpellcoreCoachToastService>(),
                _container.Resolve<StageProviderService>(),
                levelGoldReward);
        }

        public StageProcessState CreateStageProcessState()
        {
            return new StageProcessState(
                _container.Resolve<StageProviderService>(),
                _container.Resolve<MainHeroHolderService>(),
                _container.Resolve<SpellcoreProgressionService>(),
                _container.Resolve<LmbFrostProjectileService>());
        }

        public WinState CreateWinState(GameplayInputArgs inputArgs)
        {
            return new WinState(
                _container.Resolve<IInputService>(),
                _container.Resolve<PlayerDataProvider>(),
                _container.Resolve<SceneSwitcherService>(),
                _container.Resolve<ICoroutinesPerformer>(),
                _container.Resolve<StatsService>(),
                _container.Resolve<PersistedGoldRewardService>(),
                _container.Resolve<GameplayPopupService>(),
                _container.Resolve<IMouseInputService>(),
                _container.Resolve<ConfigsProviderService>().GetConfig<LevelsListConfig>().GetBy(inputArgs.LevelNumber).GoldReward
                );
        }

        public DefeatState CreateDefeatState()
        {
            return new DefeatState(
                _container.Resolve<IInputService>(),
                _container.Resolve<PlayerDataProvider>(),
                _container.Resolve<SceneSwitcherService>(),
                _container.Resolve<ICoroutinesPerformer>(),
                _container.Resolve<StatsService>(),
                _container.Resolve<IMouseInputService>(),
                _container.Resolve<GameplayPopupService>());
        }

        public GameplayStateMachine CreateGameplayStateMachine(GameplayInputArgs inputArgs)
        {
            StageProviderService stageProviderService = _container.Resolve<StageProviderService>();
            SurvivalFlowService survivalFlowService = _container.Resolve<SurvivalFlowService>();
            MainHeroHolderService mainHeroHolderService = _container.Resolve<MainHeroHolderService>();

            GameplayStateMachine coreLoopState = CreateCoreLoopState();

            DefeatState defeatState = CreateDefeatState();
            WinState winState = CreateWinState(inputArgs);

            ICompositeCondition coreLoopToWinStateCondition = new CompositeCondition()
                .Add(new FuncCondition(() => stageProviderService.CurrentStageResult.Value == StageResults.Completed))
                .Add(new FuncCondition(() => stageProviderService.HasNextStage() == false))
                .Add(new FuncCondition(() => survivalFlowService.ShouldBlockAutomaticWin == false));

            ICompositeCondition coreLoopToDefeatStateCondition = new CompositeCondition()
                .Add(new FuncCondition(() =>
                {
                    if (mainHeroHolderService.MainHero != null)
                        return mainHeroHolderService.MainHero.IsDead.Value;

                    return false;
                }));

            GameplayStateMachine gameplayCycle = new GameplayStateMachine();

            gameplayCycle.AddState(coreLoopState);
            gameplayCycle.AddState(winState);
            gameplayCycle.AddState(defeatState);

            gameplayCycle.AddTransition(coreLoopState, winState, coreLoopToWinStateCondition);
            gameplayCycle.AddTransition(coreLoopState, defeatState, coreLoopToDefeatStateCondition);

            return gameplayCycle;
        }

        public GameplayStateMachine CreateCoreLoopState()
        {
            PreparationTriggerService preparationTriggerService = _container.Resolve<PreparationTriggerService>();
            StageProviderService stageProviderService = _container.Resolve<StageProviderService>();
            PathUnlockSequenceService pathUnlockSequenceService = _container.Resolve<PathUnlockSequenceService>();

            PreparationState preparationState = CreatePreparationState();
            StageProcessState stageProcessState = CreateStageProcessState();

            ICompositeCondition preparationToStageProcessCondition = new CompositeCondition()
                .Add(new FuncCondition(() => preparationTriggerService.PrepareTriggerClicked.Value))
                .Add(new FuncCondition(() => stageProviderService.HasNextStage()))
                .Add(new FuncCondition(() => pathUnlockSequenceService.IsPlaying == false));

            FuncCondition stageProcessToPreparationCondition =
                new FuncCondition(() => stageProviderService.CurrentStageResult.Value == StageResults.Completed);

            GameplayStateMachine coreLoopState = new GameplayStateMachine();

            coreLoopState.AddState(preparationState);
            coreLoopState.AddState(stageProcessState);

            coreLoopState.AddTransition(preparationState, stageProcessState, preparationToStageProcessCondition);
            coreLoopState.AddTransition(stageProcessState, preparationState, stageProcessToPreparationCondition);

            return coreLoopState;
        }
    }
}
