using Assets._Project.Develop.Runtime.Infrastructure.DI;
using Assets._Project.Develop.Runtime.Meta.Features.Wallet;
using Assets._Project.Develop.Runtime.UI;
using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.Utilities.AssetsManagment;
using Assets._Project.Develop.Runtime.Utilities.ConfigsManagment;
using Assets._Project.Develop.Runtime.Utilities.CoroutinesManagment;
using Assets._Project.Develop.Runtime.Utilities.DataManagment;
using Assets._Project.Develop.Runtime.Utilities.DataManagment.DataProviders;
using Assets._Project.Develop.Runtime.Utilities.DataManagment.DataRepository;
using Assets._Project.Develop.Runtime.Utilities.DataManagment.KeysStorage;
using Assets._Project.Develop.Runtime.Utilities.DataManagment.Serializers;
using Assets._Project.Develop.Runtime.Utilities.LoadingScreen;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using Assets._Project.Develop.Runtime.Utilities.SceneManagment;
using Assets._Project.Develop.Runtime.Utilities.Timer;
using System;
using System.Collections.Generic;
using _Project.Develop.Runtime.Configs.Meta.Powerups;
using _Project.Develop.Runtime.Configs.Utilities.Audio;
using _Project.Develop.Runtime.Meta.Features.Powerups;
using Assets._Project.Develop.Runtime.Configs.Meta.Stats;
using Assets._Project.Develop.Runtime.Utilities.Audio;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets._Project.Develop.Runtime.Infrastructure.EntryPoint
{
    public class ProjectContextRegistrations
    {
        public static void Process(DIContainer container)
        {
            container.RegisterAsSingle<ICoroutinesPerformer>(CreateCoroutinesPerformer);

            container.RegisterAsSingle(CreateConfigsProviderService);

            container.RegisterAsSingle(CreateResourcesAssetsLoader);

            container.RegisterAsSingle(CreateSceneLoaderService);

            container.RegisterAsSingle(CreateSceneSwitcherService);

            container.RegisterAsSingle<ILoadingScreen>(CreateLoadingScreen);

            container.RegisterAsSingle(CreateWalletService).NonLazy();

            container.RegisterAsSingle(CreatePlayerDataProvider);

            container.RegisterAsSingle(CreateProjectPresentersFactory);

            container.RegisterAsSingle(CreateViewsFactory);

            container.RegisterAsSingle(CreateTimerService);

            container.RegisterAsSingle<ISaveLoadSerivce>(CreateSaveLoadService);
            
            container.RegisterAsSingle(CreateStatsService).NonLazy();
            
            container.RegisterAsSingle(CreatePowerupService).NonLazy();
            
            container.RegisterAsSingle(CreatePermanentPowerupResolver);
            
            container.RegisterAsSingle(CreateAudioHub).NonLazy();
            
            container.RegisterAsSingle<IBackgroundMusicService>(CreateBackgroundMusicService);
            
            container.RegisterAsSingle<IUISoundService>(CreateUISoundsService);
        }
        
        private static AudioHub CreateAudioHub(DIContainer c)
        {
            ResourcesAssetsLoader resources = c.Resolve<ResourcesAssetsLoader>();
            AudioHub prefab = resources.Load<AudioHub>("Utilities/AudioHub");
            
            return Object.Instantiate(prefab);
        }
        
        private static IBackgroundMusicService CreateBackgroundMusicService(DIContainer c)
        {
            AudioHub audioHub = c.Resolve<AudioHub>();
            BackgroundMusicConfig config = c.Resolve<ConfigsProviderService>()
                .GetConfig<BackgroundMusicConfig>();
            
            return new BackgroundMusicService(audioHub.BackgroundMusicSource, config);
        }
        
        private static IUISoundService CreateUISoundsService(DIContainer c)
        {
            AudioHub audioHub = c.Resolve<AudioHub>();
            UISoundsConfig config = c.Resolve<ConfigsProviderService>()
                .GetConfig<UISoundsConfig>();
            
            return new UISoundService(audioHub.UISoundsSource, config);
        }

        private static TimerServiceFactory CreateTimerService(DIContainer c)
            => new TimerServiceFactory(c);

        private static ViewsFactory CreateViewsFactory(DIContainer c)
            => new ViewsFactory(c.Resolve<ResourcesAssetsLoader>());

        private static ProjectPresentersFactory CreateProjectPresentersFactory(DIContainer c)
            => new ProjectPresentersFactory(c);
        
        private static PowerupService CreatePowerupService(DIContainer c)
        {
            return new PowerupService(c.Resolve<PlayerDataProvider>());
        }
        
        private static PermanentPowerupResolver CreatePermanentPowerupResolver(DIContainer c)
        {
            return new PermanentPowerupResolver(c.Resolve<PowerupService>(), c.Resolve<ConfigsProviderService>().GetConfig<PermanentPowerupsConfig>());
        }
        
        private static StatsService CreateStatsService(DIContainer c)
        {
            ReactiveVariable<int> wins =  new();
            ReactiveVariable<int> losses = new();

            return new StatsService(wins, losses, c.Resolve<PlayerDataProvider>());
        }

        private static PlayerDataProvider CreatePlayerDataProvider(DIContainer c)
            => new PlayerDataProvider(c.Resolve<ISaveLoadSerivce>(), c.Resolve<ConfigsProviderService>());

        private static SaveLoadService CreateSaveLoadService(DIContainer c)
        {
            IDataSerializer dataSerializer = new JsonSerializer();
            IDataKeysStorage dataKeysStorage = new MapDataKeysStorage();

            string saveFolderPath = Application.isEditor ? Application.dataPath : Application.persistentDataPath;

            IDataRepository dataRepository = new LocalFileDataRepository(saveFolderPath, "json");

            return new SaveLoadService(dataSerializer, dataKeysStorage, dataRepository);
        }

        private static WalletService CreateWalletService(DIContainer c)
        {
            Dictionary<CurrencyTypes, ReactiveVariable<int>> currencies = new();

            foreach (CurrencyTypes currencyType in Enum.GetValues(typeof(CurrencyTypes)))
                currencies[currencyType] = new ReactiveVariable<int>();

            return new WalletService(currencies, c.Resolve<PlayerDataProvider>());
        }

        private static SceneSwitcherService CreateSceneSwitcherService(DIContainer c)
            => new SceneSwitcherService(
                c.Resolve<SceneLoaderService>(),
                c.Resolve<ILoadingScreen>(),
                c);

        private static SceneLoaderService CreateSceneLoaderService(DIContainer c)
            => new SceneLoaderService();

        private static ConfigsProviderService CreateConfigsProviderService(DIContainer c)
        {
            ResourcesAssetsLoader resourcesAssetsLoader = c.Resolve<ResourcesAssetsLoader>();

            ResourcesConfigsLoader resourcesConfigsLoader = new ResourcesConfigsLoader(resourcesAssetsLoader);

            return new ConfigsProviderService(resourcesConfigsLoader);
        }

        private static ResourcesAssetsLoader CreateResourcesAssetsLoader(DIContainer c)
            => new ResourcesAssetsLoader();

        private static CoroutinesPerformer CreateCoroutinesPerformer(DIContainer c)
        {
            ResourcesAssetsLoader resourcesAssetsLoader = c.Resolve<ResourcesAssetsLoader>();

            CoroutinesPerformer coroutinesPerformerPrefab = resourcesAssetsLoader
                .Load<CoroutinesPerformer>("Utilities/CoroutinesPerformer");

            return Object.Instantiate(coroutinesPerformerPrefab);
        }

        private static StandardLoadingScreen CreateLoadingScreen(DIContainer c)
        {
            ResourcesAssetsLoader resourcesAssetsLoader = c.Resolve<ResourcesAssetsLoader>();

            StandardLoadingScreen standardLoadingScreenPrefab = resourcesAssetsLoader
                .Load<StandardLoadingScreen>("Utilities/StandardLoadingScreen");

            return Object.Instantiate(standardLoadingScreenPrefab);
        }
    }
}
