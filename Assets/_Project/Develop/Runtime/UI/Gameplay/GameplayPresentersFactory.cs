using _Project.Develop.Runtime.Gameplay.Features.InputFeature;
using _Project.Develop.Runtime.UI.Gameplay.Abilities;
using Assets._Project.Develop.Runtime.Configs.Meta.Stats;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.MainHero;
using Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature;
using Assets._Project.Develop.Runtime.Gameplay.Infrastructure;
using Assets._Project.Develop.Runtime.Infrastructure.DI;
using Assets._Project.Develop.Runtime.UI;
using Assets._Project.Develop.Runtime.UI.CommonViews;
using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.UI.Gameplay.HealthDisplay;
using Assets._Project.Develop.Runtime.UI.Gameplay.ResultsPopups;
using Assets._Project.Develop.Runtime.UI.Gameplay.Stages;
using Assets._Project.Develop.Runtime.Utilities.Audio;
using Assets._Project.Develop.Runtime.Utilities.CoroutinesManagment;
using Assets._Project.Develop.Runtime.Utilities.SceneManagment;

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
        
        public WinPopupPresenter CreateWinPopupPresenter(WinPopupView view)
        {
            return new WinPopupPresenter(
                _container.Resolve<ICoroutinesPerformer>(),
                view,
                _container.Resolve<SceneSwitcherService>(),
                _container.Resolve<IUISoundService>(),
                _container.Resolve<IBackgroundMusicService>());
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
                _container.Resolve<ProjectPresentersFactory>(),
                view,
                _container.Resolve<GameplayPresentersFactory>(),
                _container.Resolve<MainHeroHolderService>()
                );
        }

        public AbilityListPresenter CreateAbilityListPresenter(IconListView view, Entity mainHero)
        {
            return new AbilityListPresenter(_container.Resolve<MainHeroHolderService>(),
                _container.Resolve<ProjectPresentersFactory>(),
                _container.Resolve<ViewsFactory>(),
                view,
                mainHero,
                _container.Resolve<MouseOverUIService>());
        }

        public StagePresenter CreateStagePresenter(IconTextView view)
        {
            return new StagePresenter(view, _container.Resolve<StageProviderService>());
        }
        
        public EntityHealthPresenter CreateEntityHealthPresenter(Entity entity, BarWithText view)
        {
            return new EntityHealthPresenter(entity, view);
        }
        
        public EntitiesHealthDisplayPresenter CreateEntitiesHealthDisplayPresenter(EntitiesHealthDisplay view)
        {
            return new EntitiesHealthDisplayPresenter(
                _container.Resolve<EntitiesLifeContext>(),
                view,
                _container.Resolve<ViewsFactory>(),
                this);
        }
    }
}