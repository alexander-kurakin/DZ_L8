using _Project.Develop.Runtime.Configs.Utilities.Audio;
using _Project.Develop.Runtime.Gameplay.Features.Input;
using _Project.Develop.Runtime.Gameplay.Features.InputFeature;
using _Project.Develop.Runtime.Gameplay.Features.PlantableObjects;
using _Project.Develop.Runtime.Meta.Features.Powerups;
using _Project.Develop.Runtime.UI.Gameplay;
using _Project.Develop.Runtime.UI.Gameplay.LmbFlavorToast;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Essence;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Juice;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Levels;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Sectors;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Spellcore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using Assets._Project.Develop.Runtime.Gameplay.Features.Ability;
using Assets._Project.Develop.Runtime.Gameplay.Features.AI;
using Assets._Project.Develop.Runtime.Gameplay.Features.Enemies;
using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.MainHero;
using Assets._Project.Develop.Runtime.Gameplay.Features.PlantPlacementFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.PlantBuildingBuff;
using Assets._Project.Develop.Runtime.Gameplay.Features.PlantCombatFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.EssenceFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.JuiceFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.RunKillCounter;
using Assets._Project.Develop.Runtime.Gameplay.Features.GameplayTimeScale;
using Assets._Project.Develop.Runtime.Gameplay.Features.SpellcoreProgressionFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature;
using Assets._Project.Develop.Runtime.Gameplay.States;
using _Project.Develop.Runtime.Gameplay.Features.LeftClickAbilityPreview;
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

        public static void Process(DIContainer container,  GameplayInputArgs inputArgs)
        {
            _inputArgs = inputArgs;
            container.RegisterAsSingle(_ => inputArgs);

            container.RegisterAsSingle(CreateEntitiesFactory);

            container.RegisterAsSingle(CreateEntitiesLifeContext);

            container.RegisterAsSingle(CreateCollidersRegistryService);

            container.RegisterAsSingle(CreateSectorRegistryService);
            container.RegisterAsSingle(CreateSectorMembershipService);
            container.RegisterAsSingle(CreateSectorEnemyQueryService);
            container.RegisterAsSingle(CreateSectorGridFactory);
            container.RegisterAsSingle(CreateSpawnPathPreviewService);
            container.RegisterAsSingle(CreatePathUnlockSequenceService);
            container.RegisterAsSingle(CreatePlantPlacementPreviewService);
            container.RegisterAsSingle(CreatePlantPlacementPreviewController).NonLazy();
            container.RegisterAsSingle(CreateWaveSpawnPlanService);
            container.RegisterAsSingle(CreatePlantPlacementService);
            container.RegisterAsSingle(CreatePlantSellJuiceService);
            container.RegisterAsSingle(CreatePlantSellService);
            container.RegisterAsSingle(CreatePlantSellInputService);
            container.RegisterAsSingle(CreatePlantDamageCounterService);
            container.RegisterAsSingle(CreateTankMineShieldService);
            container.RegisterAsSingle(CreatePlantBuildingBuffService);
            container.RegisterAsSingle(CreatePlantBuildingBuffJuiceService);
            container.RegisterAsSingle(CreateDragonEnrageService);
            container.RegisterAsSingle(CreateScreenShakeService);
            container.RegisterAsSingle(CreateGameplayJuiceService).NonLazy();
            container.RegisterAsSingle(CreatePlantDamageApplicationService);
            container.RegisterAsSingle(CreateLmbFlavorToastService);
            container.RegisterAsSingle(CreateLmbFrostProjectileService).NonLazy();
            container.RegisterAsSingle(CreateSpellcoreProgressionService);
            container.RegisterAsSingle(CreateRunEssenceService);
            container.RegisterAsSingle(CreateEssenceFeatureService).NonLazy();
            container.RegisterAsSingle(CreateRunEnemyKillCounterService).NonLazy();
            container.RegisterAsSingle(CreateGameplayTimeScaleService).NonLazy();

            container.RegisterAsSingle(CreateBrotherRandomWalkerBrainsRegistry);
            container.RegisterAsSingle(CreateBrainsFactory);
            
            container.RegisterAsSingle(CreateAbilitiesFactory);

            container.RegisterAsSingle(CreateAIBrainsContext);

            container.RegisterAsSingle(CreateMainHeroFactory);
            container.RegisterAsSingle(CreateEnemiesFactory);

            container.RegisterAsSingle(CreateStagesFactory);
            container.RegisterAsSingle(CreateStageProviderService);
            container.RegisterAsSingle(CreateSurvivalWaveScalingService);
            container.RegisterAsSingle(CreateSurvivalFlowService);

            container.RegisterAsSingle(CreatePreparationTriggerService);

            container.RegisterAsSingle(CreateGameplayStatesFactory);

            container.RegisterAsSingle(CreateGameplayStatesContext);

            container.RegisterAsSingle(CreateMainHeroHolderService).NonLazy();

            container.RegisterAsSingle<IInputService>(CreateDesktopInput);
            
            container.RegisterAsSingle<IMouseInputService>(CreateMouseInput);
            
            container.RegisterAsSingle(CreateMouseRaycastService);
            
            container.RegisterAsSingle(CreateMouseOverUIService);

            container.RegisterAsSingle(CreateMonoEntitiesFactory).NonLazy();
            
            container.RegisterAsSingle(CreateGameplayUIRoot).NonLazy();
            
            container.RegisterAsSingle(CreateGameplayPresentersFactory);
            
            container.RegisterAsSingle(CreateGameplayScreenPresenter).NonLazy();
            
            container.RegisterAsSingle(CreateGameplayPopupService);
            
            container.RegisterAsSingle(CreatePowerupFactory);
            
            container.RegisterAsSingle(CreatePlantableObjectsFactory);
            
            container.RegisterAsSingle<IGameSoundsService>(CreateGameSoundsService);
        }
        
        private static IGameSoundsService CreateGameSoundsService(DIContainer c)
        {
            AudioHub audioHub = c.Resolve<AudioHub>();
            GameSoundsConfig config = c.Resolve<ConfigsProviderService>()
                .GetConfig<GameSoundsConfig>();
            
            return new GameSoundsService(audioHub.GameSoundSource, config);
        }
        
        private static PlantableObjectsFactory CreatePlantableObjectsFactory(DIContainer c)
        {
            return new PlantableObjectsFactory(c);
        }
        
        private static PowerupFactory CreatePowerupFactory(DIContainer c)
        {
            return new PowerupFactory(c);
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

        private static PreparationTriggerService CreatePreparationTriggerService(DIContainer c)
        {
            return new PreparationTriggerService(
                c.Resolve<EntitiesFactory>(),
                c.Resolve<EntitiesLifeContext>(),
                c.Resolve<IMouseInputService>(),
                c.Resolve<MouseRaycastService>(),
                c.Resolve<ConfigsProviderService>());
        }

        private static StageProviderService CreateStageProviderService(DIContainer c)
        {
            return new StageProviderService(
                c.Resolve<ConfigsProviderService>().GetConfig<LevelsListConfig>().GetBy(_inputArgs.LevelNumber),
                c.Resolve<StagesFactory>(),
                c.Resolve<SurvivalWaveScalingService>(),
                c.Resolve<EntitiesLifeContext>());
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

        private static BrotherRandomWalkerBrainsRegistry CreateBrotherRandomWalkerBrainsRegistry(DIContainer c)
        {
            return new BrotherRandomWalkerBrainsRegistry();
        }

        private static BrainsFactory CreateBrainsFactory(DIContainer c)
        {
            return new BrainsFactory(c);
        }

        private static CollidersRegistryService CreateCollidersRegistryService(DIContainer c)
        {
            return new CollidersRegistryService();
        }

        private static SectorRegistryService CreateSectorRegistryService(DIContainer c)
        {
            return new SectorRegistryService();
        }

        private static SectorMembershipService CreateSectorMembershipService(DIContainer c)
        {
            return new SectorMembershipService(c.Resolve<SectorRegistryService>());
        }

        private static SectorEnemyQueryService CreateSectorEnemyQueryService(DIContainer c)
        {
            return new SectorEnemyQueryService(
                c.Resolve<EntitiesLifeContext>(),
                c.Resolve<SectorMembershipService>());
        }

        private static LmbFlavorToastService CreateLmbFlavorToastService(DIContainer c)
        {
            return new LmbFlavorToastService();
        }

        private static LmbFrostProjectileService CreateLmbFrostProjectileService(DIContainer c)
        {
            return new LmbFrostProjectileService();
        }

        private static SectorGridFactory CreateSectorGridFactory(DIContainer c)
        {
            return new SectorGridFactory(c);
        }

        private static PlantPlacementService CreatePlantPlacementService(DIContainer c)
        {
            return new PlantPlacementService(
                c.Resolve<SectorRegistryService>(),
                c.Resolve<SectorMembershipService>(),
                c.Resolve<EntitiesLifeContext>());
        }

        private static PlantSellJuiceService CreatePlantSellJuiceService(DIContainer c)
        {
            return new PlantSellJuiceService(
                c.Resolve<ConfigsProviderService>().GetConfig<EssenceConfig>());
        }

        private static PlantSellService CreatePlantSellService(DIContainer c)
        {
            return new PlantSellService(
                c.Resolve<PlantPlacementService>(),
                c.Resolve<SectorMembershipService>(),
                c.Resolve<RunEssenceService>(),
                c.Resolve<ConfigsProviderService>().GetConfig<EssenceConfig>(),
                c.Resolve<EntitiesLifeContext>(),
                c.Resolve<PlantSellJuiceService>());
        }

        private static PlantSellInputService CreatePlantSellInputService(DIContainer c)
        {
            return new PlantSellInputService(
                c.Resolve<PlantPlacementPreviewService>(),
                c.Resolve<PlantSellService>());
        }

        private static PlantDamageCounterService CreatePlantDamageCounterService(DIContainer c)
        {
            return new PlantDamageCounterService();
        }

        private static TankMineShieldService CreateTankMineShieldService(DIContainer c)
        {
            return new TankMineShieldService();
        }

        private static PlantBuildingBuffService CreatePlantBuildingBuffService(DIContainer c)
        {
            return new PlantBuildingBuffService(
                c.Resolve<PlantPlacementService>(),
                c.Resolve<SectorMembershipService>(),
                c.Resolve<ConfigsProviderService>().GetConfig<SpellcoreCombatConfig>(),
                c.Resolve<EntitiesLifeContext>(),
                c.Resolve<RunEssenceService>());
        }

        private static PlantBuildingBuffJuiceService CreatePlantBuildingBuffJuiceService(DIContainer c)
        {
            return new PlantBuildingBuffJuiceService(
                c.Resolve<PlantBuildingBuffService>(),
                c.Resolve<ConfigsProviderService>().GetConfig<BuildingBuffCastAbilityConfig>(),
                c.Resolve<IGameSoundsService>());
        }

        private static DragonEnrageService CreateDragonEnrageService(DIContainer c)
        {
            return new DragonEnrageService(
                c.Resolve<ConfigsProviderService>().GetConfig<DragonEnrageConfig>());
        }

        private static ScreenShakeService CreateScreenShakeService(DIContainer c)
        {
            return new ScreenShakeService(c.Resolve<ConfigsProviderService>().GetConfig<GameplayVfxConfig>());
        }

        private static GameplayJuiceService CreateGameplayJuiceService(DIContainer c)
        {
            return new GameplayJuiceService(
                c.Resolve<EntitiesLifeContext>(),
                c.Resolve<ConfigsProviderService>().GetConfig<DragonEnrageConfig>(),
                c.Resolve<ScreenShakeService>());
        }

        private static PlantDamageApplicationService CreatePlantDamageApplicationService(DIContainer c)
        {
            return new PlantDamageApplicationService(
                c.Resolve<PlantDamageCounterService>(),
                c.Resolve<TankMineShieldService>(),
                c.Resolve<ConfigsProviderService>().GetConfig<SpellcoreCombatConfig>(),
                c.Resolve<PlantBuildingBuffService>());
        }

        private static SpawnPathPreviewService CreateSpawnPathPreviewService(DIContainer c)
        {
            return new SpawnPathPreviewService();
        }

        private static PathUnlockSequenceService CreatePathUnlockSequenceService(DIContainer c)
        {
            return new PathUnlockSequenceService(c.Resolve<SpawnPathPreviewService>());
        }

        private static PlantPlacementPreviewService CreatePlantPlacementPreviewService(DIContainer c)
        {
            return new PlantPlacementPreviewService();
        }

        private static PlantPlacementPreviewController CreatePlantPlacementPreviewController(DIContainer c)
        {
            return new PlantPlacementPreviewController(
                c.Resolve<PlantPlacementPreviewService>(),
                c.Resolve<SectorRegistryService>(),
                c.Resolve<PlantPlacementService>(),
                c.Resolve<MainHeroHolderService>(),
                c.Resolve<SpellcoreProgressionService>(),
                c.Resolve<PathUnlockSequenceService>());
        }

        private static WaveSpawnPlanService CreateWaveSpawnPlanService(DIContainer c)
        {
            return new WaveSpawnPlanService();
        }

        private static SurvivalWaveScalingService CreateSurvivalWaveScalingService(DIContainer c)
        {
            return new SurvivalWaveScalingService();
        }

        private static SurvivalFlowService CreateSurvivalFlowService(DIContainer c)
        {
            return new SurvivalFlowService(c.Resolve<StageProviderService>());
        }

        private static SpellcoreProgressionService CreateSpellcoreProgressionService(DIContainer c)
        {
            return new SpellcoreProgressionService(
                c.Resolve<ConfigsProviderService>().GetConfig<SpellcoreProgressionConfig>(),
                c.Resolve<SectorRegistryService>(),
                c.Resolve<StageProviderService>(),
                c.Resolve<SurvivalFlowService>(),
                c.Resolve<ConfigsProviderService>(),
                c.Resolve<SpawnPathPreviewService>(),
                c.Resolve<WaveSpawnPlanService>(),
                c.Resolve<PathUnlockSequenceService>(),
                c.Resolve<ConfigsProviderService>().GetConfig<SpellcoreCombatConfig>());
        }

        private static RunEssenceService CreateRunEssenceService(DIContainer c)
        {
            return new RunEssenceService();
        }

        private static EssenceFeatureService CreateEssenceFeatureService(DIContainer c)
        {
            return new EssenceFeatureService(
                c.Resolve<ConfigsProviderService>(),
                c.Resolve<RunEssenceService>(),
                c.Resolve<EntitiesLifeContext>(),
                c.Resolve<MainHeroHolderService>(),
                c.Resolve<IMouseInputService>(),
                c.Resolve<MouseRaycastService>());
        }

        private static RunEnemyKillCounterService CreateRunEnemyKillCounterService(DIContainer c)
        {
            return new RunEnemyKillCounterService(c.Resolve<EntitiesLifeContext>());
        }

        private static GameplayTimeScaleService CreateGameplayTimeScaleService(DIContainer c)
        {
            return new GameplayTimeScaleService();
        }

        private static MonoEntitiesFactory CreateMonoEntitiesFactory(DIContainer c)
        {
            return new MonoEntitiesFactory(
                c.Resolve<ResourcesAssetsLoader>(),
                c.Resolve<EntitiesLifeContext>(),
                c.Resolve<CollidersRegistryService>(),
                c.Resolve<IGameSoundsService>());
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

            LmbFlavorToastPresenter toastPresenter = c
                .Resolve<GameplayPresentersFactory>()
                .CreateLmbFlavorToastPresenter(gameplayUIRoot.HUDLayer);

            toastPresenter.Initialize();

            return presenter;
        }
    }
}
