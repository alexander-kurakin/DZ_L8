using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;
using _Project.Develop.Runtime.UI.Gameplay.Abilities;

using Assets._Project.Develop.Runtime.Configs.Meta.Stats;

using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;

using Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature;

using Assets._Project.Develop.Runtime.Gameplay.Features.RunKillCounter;

using Assets._Project.Develop.Runtime.Gameplay.Features.WaveProgressFeature;

using Assets._Project.Develop.Runtime.Gameplay.Infrastructure;

using Assets._Project.Develop.Runtime.Utilities.ConfigsManagment;

using Assets._Project.Develop.Runtime.Gameplay.States;

using Assets._Project.Develop.Runtime.Infrastructure.DI;

using Assets._Project.Develop.Runtime.UI;

using Assets._Project.Develop.Runtime.UI.CommonViews;

using Assets._Project.Develop.Runtime.UI.Core;

using Assets._Project.Develop.Runtime.UI.Gameplay.ResultsPopups;

using Assets._Project.Develop.Runtime.UI.Stats;

using Assets._Project.Develop.Runtime.UI.Gameplay.Stages;

using Assets._Project.Develop.Runtime.Utilities.Audio;

using Assets._Project.Develop.Runtime.Utilities.CoroutinesManagment;

using Assets._Project.Develop.Runtime.Utilities.SceneManagment;

using UnityEngine;



namespace _Project.Develop.Runtime.UI.Gameplay

{

    public class GameplayPresentersFactory

    {

        private readonly DIContainer _container;

        private readonly GameplayInputArgs _gameplayInputArgs;



        public GameplayPresentersFactory(DIContainer container, GameplayInputArgs gameplayInputArgs)

        {

            _container = container;

            _gameplayInputArgs = gameplayInputArgs;

        }



        public WinPopupPresenter CreateWinPopupPresenter(WinPopupView view, WinPopupOpenArgs openArgs)

        {

            return new WinPopupPresenter(

                _container.Resolve<ICoroutinesPerformer>(),

                view,

                _container.Resolve<SceneSwitcherService>(),

                _container.Resolve<IUISoundService>(),

                _container.Resolve<IBackgroundMusicService>(),

                openArgs);

        }



        public DefeatPopupPresenter CreateDefeatPopupPresenter(DefeatPopupView view)

        {

            return new DefeatPopupPresenter(

                _container.Resolve<ICoroutinesPerformer>(),

                view,

                _container.Resolve<SceneSwitcherService>(),

                _gameplayInputArgs,

                _container.Resolve<IUISoundService>(),

                _container.Resolve<IBackgroundMusicService>());

        }



        public GameplayScreenPresenter CreateGameplayScreen(GameplayScreenView view)

        {

            return new GameplayScreenPresenter(

                view,

                _container.Resolve<GameplayPresentersFactory>(),

                _container.Resolve<PlayerModifiersHolderService>());

        }



        public ModifierListPresenter CreateModifierListPresenter(AbilitySlotListView view, Entity playerEntity)

        {

            return new ModifierListPresenter(

                _container.Resolve<ProjectPresentersFactory>(),

                _container.Resolve<ViewsFactory>(),

                view,

                playerEntity,

                _container.Resolve<MouseOverUIService>());

        }



        public GameplayStatsPresenter CreateGameplayStatsPresenter(IconTextListView view)

        {

            return new GameplayStatsPresenter(

                view,

                _container.Resolve<ViewsFactory>(),

                _container.Resolve<RunEnemyKillCounterService>(),

                _container.Resolve<ConfigsProviderService>().GetConfig<StatIconsConfig>());

        }



        public StagePresenter CreateStagePresenter(IconTextView view)
        {
            return new StagePresenter(
                view,
                _container.Resolve<StageProviderService>(),
                _container.Resolve<GameplayPhaseService>(),
                _container.Resolve<WaveProgressService>());
        }
    }
}

