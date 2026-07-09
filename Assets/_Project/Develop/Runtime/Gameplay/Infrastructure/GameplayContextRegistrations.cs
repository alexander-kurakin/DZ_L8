using _Project.Develop.Runtime.UI.Gameplay;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Gnome;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Levels;
using Assets._Project.Develop.Runtime.Configs.Gameplay.MouseConfig;
using Assets._Project.Develop.Runtime.Configs.Gameplay.ProjectileModifiers;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Waves;
using Assets._Project.Develop.Runtime.Configs.Meta.Stats;
using Assets._Project.Develop.Runtime.Configs.Meta.Wallet;
using Assets._Project.Develop.Runtime.Configs.Utilities.Audio;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using Assets._Project.Develop.Runtime.Gameplay.Features.AI;
using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.MainHero;
using Assets._Project.Develop.Runtime.Gameplay.Features.GnomeFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.ProjectileFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.ProjectileModifiers;
using Assets._Project.Develop.Runtime.Gameplay.Features.RunKillCounter;
using Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.WaveProgressFeature;
using Assets._Project.Develop.Runtime.Gameplay.States;
using Assets._Project.Develop.Runtime.Infrastructure.DI;
using Assets._Project.Develop.Runtime.UI;
using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.Utilities.AssetsManagment;
using Assets._Project.Develop.Runtime.Utilities.Audio;
using Assets._Project.Develop.Runtime.Utilities.ConfigsManagment;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Infrastructure
{
    public class GameplayContextRegistrations
    {
        private static GameplayInputArgs _inputArgs;

        public static void Process(DIContainer container, GameplayInputArgs inputArgs)
        {
            _inputArgs = inputArgs;
            container.RegisterAsSingle(_ => inputArgs);

            container.RegisterAsSingle(CreateEntitiesFactory);
            container.RegisterAsSingle(CreateEntitiesLifeContext);
            container.RegisterAsSingle(CreateCollidersRegistryService);
            container.RegisterAsSingle(CreateProjectileModifiersFactory);
            container.RegisterAsSingle(CreateProjectileSpawnService);
            container.RegisterAsSingle(CreateStagesFactory);
            container.RegisterAsSingle(CreateStageProviderService);
            container.RegisterAsSingle(CreateWaveProgressService).NonLazy();
            container.RegisterAsSingle(CreateGameplayPhaseService);
            container.RegisterAsSingle(CreatePreparationTriggerService);
            container.RegisterAsSingle(CreatePlayerModifiersHolderService);
            container.RegisterAsSingle(CreateGameplayStatesFactory);
            container.RegisterAsSingle(CreateGameplayStatesContext);
            container.RegisterAsSingle<IInputService>(CreateDesktopInput);
            container.RegisterAsSingle<IMouseInputService>(CreateMouseInput);
            container.RegisterAsSingle<IMouseRaycastService>(CreateMouseRaycastService);
            container.RegisterAsSingle(CreateMouseOverUIService);
            container.RegisterAsSingle(CreateMonoEntitiesFactory).NonLazy();
            container.RegisterAsSingle(CreateRunEnemyKillCounterService).NonLazy();
            container.RegisterAsSingle(CreateGameplayUIRoot).NonLazy();
            container.RegisterAsSingle(CreateGameplayPresentersFactory);
            container.RegisterAsSingle(CreateGameplayScreenPresenter).NonLazy();
            container.RegisterAsSingle(CreateGameplayPopupService);
            container.RegisterAsSingle<IGameSoundsService>(CreateGameSoundsService);
            
            container.RegisterAsSingle(CreateMainHeroFactory);
            container.RegisterAsSingle(CreateAIBrainsContext);
            container.RegisterAsSingle(CreateBrainsFactory);
            container.RegisterAsSingle(CreateEnemiesFactory);
            container.RegisterAsSingle(CreateGnomeOrchestratorService);
            container.RegisterAsSingle(CreateGnomeWinConditionService).NonLazy();
            container.RegisterAsSingle(CreateGnomePeekPointsHolder);
            
            container.RegisterAsSingle(CreateMainHeroHolderService).NonLazy();
        }
        
        private static AIBrainsContext CreateAIBrainsContext(DIContainer c)
        {
            return new AIBrainsContext();
        }
        
        private static BrainsFactory CreateBrainsFactory(DIContainer c)
        {
            return new BrainsFactory(c);
        }
        
        private static MainHeroFactory CreateMainHeroFactory(DIContainer c)
        {
            return new MainHeroFactory(c, _inputArgs.LevelNumber);
        }
        
        private static MainHeroHolderService CreateMainHeroHolderService(DIContainer c)
        {
            return new MainHeroHolderService(c.Resolve<EntitiesLifeContext>());
        }

        private static WaveProgressService CreateWaveProgressService(DIContainer container)
        {
            return new WaveProgressService(
                container.Resolve<StageProviderService>(),
                container.Resolve<ConfigsProviderService>().GetConfig<WaveProgressConfig>());
        }

        private static GameplayPhaseService CreateGameplayPhaseService(DIContainer container)
        {
            return new GameplayPhaseService();
        }

        private static PlayerModifiersHolderService CreatePlayerModifiersHolderService(DIContainer container)
        {
            return new PlayerModifiersHolderService(container.Resolve<ProjectileModifiersFactory>());
        }

        private static ProjectileModifiersFactory CreateProjectileModifiersFactory(DIContainer container)
        {
            return new ProjectileModifiersFactory(container);
        }

        private static ProjectileSpawnService CreateProjectileSpawnService(DIContainer container)
        {
            return new ProjectileSpawnService(container);
        }

        private static IGameSoundsService CreateGameSoundsService(DIContainer container)
        {
            AudioHub audioHub = container.Resolve<AudioHub>();
            GameSoundsConfig config = container.Resolve<ConfigsProviderService>().GetConfig<GameSoundsConfig>();
            return new GameSoundsService(audioHub.GameSoundSource, config);
        }

        private static GameplayPopupService CreateGameplayPopupService(DIContainer container)
        {
            return new GameplayPopupService(
                container.Resolve<ViewsFactory>(),
                container.Resolve<ProjectPresentersFactory>(),
                container.Resolve<GameplayUIRoot>(),
                container.Resolve<GameplayPresentersFactory>());
        }

        private static MouseOverUIService CreateMouseOverUIService(DIContainer container)
        {
            return new MouseOverUIService();
        }

        private static MouseRaycastService CreateMouseRaycastService(DIContainer container)
        {
            return new MouseRaycastService(
                Camera.main,
                container.Resolve<ConfigsProviderService>().GetConfig<RaycastConfig>());
        }

        private static MouseInput CreateMouseInput(DIContainer container)
        {
            return new MouseInput();
        }

        private static DesktopInput CreateDesktopInput(DIContainer container)
        {
            return new DesktopInput();
        }

        private static GameplayStatesContext CreateGameplayStatesContext(DIContainer container)
        {
            return new GameplayStatesContext(container.Resolve<GameplayStatesFactory>().CreateGameplayStateMachine(_inputArgs));
        }

        private static GameplayStatesFactory CreateGameplayStatesFactory(DIContainer container)
        {
            return new GameplayStatesFactory(container);
        }

        private static PreparationTriggerService CreatePreparationTriggerService(DIContainer container)
        {
            return new PreparationTriggerService();
        }

        private static StageProviderService CreateStageProviderService(DIContainer container)
        {
            return new StageProviderService(
                container.Resolve<ConfigsProviderService>().GetConfig<LevelsListConfig>().GetBy(_inputArgs.LevelNumber),
                container.Resolve<StagesFactory>(),
                container.Resolve<EntitiesLifeContext>());
        }

        private static StagesFactory CreateStagesFactory(DIContainer container)
        {
            return new StagesFactory(container);
        }

        private static CollidersRegistryService CreateCollidersRegistryService(DIContainer container)
        {
            return new CollidersRegistryService();
        }

        private static RunEnemyKillCounterService CreateRunEnemyKillCounterService(DIContainer container)
        {
            return new RunEnemyKillCounterService(container.Resolve<EntitiesLifeContext>());
        }

        private static MonoEntitiesFactory CreateMonoEntitiesFactory(DIContainer container)
        {
            return new MonoEntitiesFactory(
                container.Resolve<ResourcesAssetsLoader>(),
                container.Resolve<EntitiesLifeContext>(),
                container.Resolve<CollidersRegistryService>(),
                container.Resolve<IGameSoundsService>());
        }

        private static EnemiesFactory CreateEnemiesFactory(DIContainer container)
        {
            return new EnemiesFactory(container);
        }

        private static GnomeOrchestratorService CreateGnomeOrchestratorService(DIContainer container)
        {
            return new GnomeOrchestratorService(
                container.Resolve<EnemiesFactory>(),
                container.Resolve<EntitiesLifeContext>(),
                container.Resolve<IMouseRaycastService>(),
                container.Resolve<IMouseInputService>(),
                container.Resolve<ConfigsProviderService>().GetConfig<GnomeArenaConfig>());
        }

        private static GnomeWinConditionService CreateGnomeWinConditionService(DIContainer container)
        {
            return new GnomeWinConditionService(
                container.Resolve<RunEnemyKillCounterService>(),
                container.Resolve<ConfigsProviderService>().GetConfig<GnomeArenaConfig>());
        }

        private static GnomePeekPointsHolder CreateGnomePeekPointsHolder(DIContainer container)
        {
            return new GnomePeekPointsHolder();
        }

        private static EntitiesLifeContext CreateEntitiesLifeContext(DIContainer container)
        {
            return new EntitiesLifeContext();
        }

        private static EntitiesFactory CreateEntitiesFactory(DIContainer container)
        {
            return new EntitiesFactory(container);
        }

        private static GameplayUIRoot CreateGameplayUIRoot(DIContainer container)
        {
            ResourcesAssetsLoader resourcesAssetsLoader = container.Resolve<ResourcesAssetsLoader>();

            GameplayUIRoot gameplayUIRootPrefab = resourcesAssetsLoader
                .Load<GameplayUIRoot>("UI/Gameplay/GameplayUIRoot");

            return Object.Instantiate(gameplayUIRootPrefab);
        }

        private static GameplayPresentersFactory CreateGameplayPresentersFactory(DIContainer container)
            => new GameplayPresentersFactory(container, _inputArgs);

        private static GameplayScreenPresenter CreateGameplayScreenPresenter(DIContainer container)
        {
            GameplayUIRoot gameplayUIRoot = container.Resolve<GameplayUIRoot>();

            GameplayScreenView view = container
                .Resolve<ViewsFactory>()
                .Create<GameplayScreenView>(ViewIDs.GameplayScreen, gameplayUIRoot.HUDLayer);

            return container
                .Resolve<GameplayPresentersFactory>()
                .CreateGameplayScreen(view);
        }
    }
}
