using _Project.Develop.Runtime.Gameplay.Features.Input;
using _Project.Develop.Runtime.Gameplay.Features.InputFeature;
using _Project.Develop.Runtime.UI.Gameplay;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Levels;
using Assets._Project.Develop.Runtime.Configs.Meta.Stats;
using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.MainHero;
using Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature;
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
            return new PreparationState(
                _container.Resolve<PreparationTriggerService>(), 
                _container.Resolve<ConfigsProviderService>(),
                _container.Resolve<MainHeroHolderService>(),
                _container.Resolve<MouseRaycastService>(),
                _container.Resolve<MouseInput>(),
                _container.Resolve<IBackgroundMusicService>(),
                _container.Resolve<MouseOverUIService>());
        }

        public StageProcessState CreateStageProcessState()
        {
            return new StageProcessState(
                _container.Resolve<StageProviderService>(),
                _container.Resolve<MainHeroHolderService>());
        }

        public WinState CreateWinState(GameplayInputArgs inputArgs)
        {
            return new WinState(
                _container.Resolve<IInputService>(),
                _container.Resolve<PlayerDataProvider>(),
                _container.Resolve<SceneSwitcherService>(),
                _container.Resolve<ICoroutinesPerformer>(),
                _container.Resolve<StatsService>(),
                _container.Resolve<WalletService>(),
                _container.Resolve<GameplayPopupService>(),
                _container.Resolve<ConfigsProviderService>().GetConfig<LevelsListConfig>().GetBy(inputArgs.LevelNumber).GoldReward,
                _container.Resolve<ConfigsProviderService>().GetConfig<LevelsListConfig>().GetBy(inputArgs.LevelNumber).DiamondRewardMin,
                _container.Resolve<ConfigsProviderService>().GetConfig<LevelsListConfig>().GetBy(inputArgs.LevelNumber).DiamondRewardMax
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
                _container.Resolve<GameplayPopupService>());
        }

        public GameplayStateMachine CreateGameplayStateMachine(GameplayInputArgs inputArgs)
        {
            PreparationTriggerService preparationTriggerService = _container.Resolve<PreparationTriggerService>();
            StageProviderService stageProviderService = _container.Resolve<StageProviderService>();
            MainHeroHolderService mainHeroHolderService = _container.Resolve<MainHeroHolderService>();

            GameplayStateMachine coreLoopState = CreateCoreLoopState();

            DefeatState defeatState = CreateDefeatState();
            WinState winState = CreateWinState(inputArgs);

            ICompositeCondition coreLoopToWinStateCondition = new CompositeCondition()
                .Add(new FuncCondition(() => preparationTriggerService.PrepareTriggerClicked.Value))
                .Add(new FuncCondition(() => stageProviderService.CurrentStageResult.Value == StageResults.Completed))
                .Add(new FuncCondition(() => stageProviderService.HasNextStage() == false));

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

            PreparationState preparationState = CreatePreparationState();
            StageProcessState stageProcessState = CreateStageProcessState();

            ICompositeCondition preparationToStageProcessCondition = new CompositeCondition()
                .Add(new FuncCondition(() => preparationTriggerService.PrepareTriggerClicked.Value))
                .Add(new FuncCondition(() => stageProviderService.HasNextStage()));

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
