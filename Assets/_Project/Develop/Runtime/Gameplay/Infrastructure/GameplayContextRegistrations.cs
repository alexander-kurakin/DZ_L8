using _Project.Develop.Runtime.Gameplay.Features.Input;
using _Project.Develop.Runtime.Gameplay.Features.InputFeature;
using _Project.Develop.Runtime.UI.Gameplay;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Levels;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using Assets._Project.Develop.Runtime.Gameplay.Features.Ability;
using Assets._Project.Develop.Runtime.Gameplay.Features.AI;
using Assets._Project.Develop.Runtime.Gameplay.Features.Enemies;
using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.MainHero;
using Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature;
using Assets._Project.Develop.Runtime.Gameplay.States;
using Assets._Project.Develop.Runtime.Infrastructure.DI;
using Assets._Project.Develop.Runtime.UI;
using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.Utilities.AssetsManagment;
using Assets._Project.Develop.Runtime.Utilities.ConfigsManagment;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Infrastructure
{
    public class GameplayContextRegistrations
    {
        
        private static GameplayInputArgs _inputArgs;

        public static void Process(DIContainer container,  GameplayInputArgs inputArgs)
        {
            _inputArgs = inputArgs;
            
            container.RegisterAsSingle(CreateEntitiesFactory);

            container.RegisterAsSingle(CreateEntitiesLifeContext);

            container.RegisterAsSingle(CreateCollidersRegistryService);

            container.RegisterAsSingle(CreateBrainsFactory);
            
            container.RegisterAsSingle(CreateAbilitiesFactory);

            container.RegisterAsSingle(CreateAIBrainsContext);

            container.RegisterAsSingle(CreateMainHeroFactory);
            container.RegisterAsSingle(CreateEnemiesFactory);

            container.RegisterAsSingle(CreateStagesFactory);
            container.RegisterAsSingle(CreateStageProviderService);

            container.RegisterAsSingle(CreatePreperationTriggerService);

            container.RegisterAsSingle(CreateGameplayStatesFactory);

            container.RegisterAsSingle(CreateGameplayStatesContext);

            container.RegisterAsSingle(CreateMainHeroHolderService).NonLazy();

            container.RegisterAsSingle<IInputService>(CreateDesktopInput);
            
            container.RegisterAsSingle(CreateMouseInput);
            
            container.RegisterAsSingle(CreateMouseRaycastService);
            
            container.RegisterAsSingle(CreateMouseOverUIService);

            container.RegisterAsSingle(CreateMonoEntitiesFactory).NonLazy();
            
            container.RegisterAsSingle(CreateGameplayUIRoot).NonLazy();
            
            container.RegisterAsSingle(CreateGameplayPresentersFactory);
            
            container.RegisterAsSingle(CreateGameplayScreenPresenter).NonLazy();
            
            container.RegisterAsSingle(CreateGameplayPopupService);
            
        }
        
        private static GameplayPopupService CreateGameplayPopupService(DIContainer c)
        {
            return new GameplayPopupService(
                c.Resolve<ViewsFactory>(),
                c.Resolve<ProjectPresentersFactory>(),
                c.Resolve<GameplayUIRoot>(),
                c.Resolve<GameplayPresentersFactory>());
        }

        private static AbilitiesFactory CreateAbilitiesFactory(DIContainer c)
        {
            return new  AbilitiesFactory(c);
        }
        
        private static MouseOverUIService CreateMouseOverUIService(DIContainer c)
        {
            return new MouseOverUIService();
        }

        private static MouseRaycastService CreateMouseRaycastService(DIContainer c)
        {
            return new MouseRaycastService(Camera.main);
        }
        
        private static MouseInput CreateMouseInput(DIContainer c)
        {
            return new MouseInput();
        }
        
        private static GameplayStatesContext CreateGameplayStatesContext(DIContainer c)
        {
            return new GameplayStatesContext(c.Resolve<GameplayStatesFactory>().CreateGameplayStateMachine(_inputArgs));
        }

        private static GameplayStatesFactory CreateGameplayStatesFactory(DIContainer c)
        {
            return new GameplayStatesFactory(c);
        }

        private static MainHeroHolderService CreateMainHeroHolderService(DIContainer c)
        {
            return new MainHeroHolderService(c.Resolve<EntitiesLifeContext>());
        }

        private static PreparationTriggerService CreatePreperationTriggerService(DIContainer c)
        {
            return new PreparationTriggerService(
                c.Resolve<EntitiesFactory>(),
                c.Resolve<EntitiesLifeContext>(),
                c.Resolve<MouseInput>(),
                c.Resolve<MouseRaycastService>(),
                c.Resolve<ConfigsProviderService>());
        }

        private static StageProviderService CreateStageProviderService(DIContainer c)
        {
            return new StageProviderService(
                c.Resolve<ConfigsProviderService>().GetConfig<LevelsListConfig>().GetBy(_inputArgs.LevelNumber),
                c.Resolve<StagesFactory>());
        }

        private static StagesFactory CreateStagesFactory(DIContainer c)
        {
            return new StagesFactory(c);
        }

        private static EnemiesFactory CreateEnemiesFactory(DIContainer c)
        {
            return new EnemiesFactory(c);
        }

        private static MainHeroFactory CreateMainHeroFactory(DIContainer c)
        {
            return new MainHeroFactory(c, _inputArgs.LevelNumber);
        }

        private static DesktopInput CreateDesktopInput(DIContainer c)
        {
            return new DesktopInput();
        }

        private static AIBrainsContext CreateAIBrainsContext(DIContainer c)
        {
            return new AIBrainsContext();
        }

        private static BrainsFactory CreateBrainsFactory(DIContainer c)
        {
            return new BrainsFactory(c);
        }

        private static CollidersRegistryService CreateCollidersRegistryService(DIContainer c)
        {
            return new CollidersRegistryService();
        }

        private static MonoEntitiesFactory CreateMonoEntitiesFactory(DIContainer c)
        {
            return new MonoEntitiesFactory(
                c.Resolve<ResourcesAssetsLoader>(),
                c.Resolve<EntitiesLifeContext>(),
                c.Resolve<CollidersRegistryService>());
        }

        private static EntitiesLifeContext CreateEntitiesLifeContext(DIContainer c)
        {
            return new EntitiesLifeContext();
        }

        private static EntitiesFactory CreateEntitiesFactory(DIContainer c)
        {
            return new EntitiesFactory(c);
        }
        
        private static GameplayUIRoot CreateGameplayUIRoot(DIContainer c)
        {
            ResourcesAssetsLoader resourcesAssetsLoader = c.Resolve<ResourcesAssetsLoader>();

            GameplayUIRoot gameplayUIRootPrefab = resourcesAssetsLoader
                .Load<GameplayUIRoot>("UI/Gameplay/GameplayUIRoot");

            return Object.Instantiate(gameplayUIRootPrefab);
        }

        private static GameplayPresentersFactory CreateGameplayPresentersFactory(DIContainer c)
            => new GameplayPresentersFactory(c, _inputArgs);

        private static GameplayScreenPresenter CreateGameplayScreenPresenter(DIContainer c)
        {
            GameplayUIRoot gameplayUIRoot = c.Resolve<GameplayUIRoot>();
            
            GameplayScreenView view = c
                .Resolve<ViewsFactory>()
                .Create<GameplayScreenView>(ViewIDs.GameplayScreen, gameplayUIRoot.HUDLayer);

            GameplayScreenPresenter presenter = c
                .Resolve<GameplayPresentersFactory>()
                .CreateGameplayScreen(view);

            return presenter;
        }
    }
}
