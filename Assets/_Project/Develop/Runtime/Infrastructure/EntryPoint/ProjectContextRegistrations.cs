using Assets._Project.Develop.Runtime.Infrastructure.DI;
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
using Assets._Project.Develop.Runtime.Configs.Utilities.Audio;
using Assets._Project.Develop.Runtime.Configs.Meta.Stats;
using Assets._Project.Develop.Runtime.Utilities.Audio;
using Assets._Project.Develop.Runtime.Meta.Features.Wallet;
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
            container.RegisterAsSingle(CreatePersistedGoldRewardService);
            container.RegisterAsSingle(CreateProjectPresentersFactory);
            container.RegisterAsSingle(CreateViewsFactory);
            container.RegisterAsSingle(CreateTimerService);
            container.RegisterAsSingle<ISaveLoadSerivce>(CreateSaveLoadService);
            container.RegisterAsSingle(CreateStatsService).NonLazy();
            container.RegisterAsSingle(CreateAudioHub).NonLazy();
            container.RegisterAsSingle<IBackgroundMusicService>(CreateBackgroundMusicService);
            container.RegisterAsSingle<IUISoundService>(CreateUISoundsService);
        }

        private static AudioHub CreateAudioHub(DIContainer container)
        {
            ResourcesAssetsLoader resources = container.Resolve<ResourcesAssetsLoader>();
            AudioHub prefab = resources.Load<AudioHub>("Utilities/AudioHub");
            return Object.Instantiate(prefab);
        }

        private static IBackgroundMusicService CreateBackgroundMusicService(DIContainer container)
        {
            AudioHub audioHub = container.Resolve<AudioHub>();
            BackgroundMusicConfig config = container.Resolve<ConfigsProviderService>()
                .GetConfig<BackgroundMusicConfig>();

            return new BackgroundMusicService(audioHub.BackgroundMusicSource, config);
        }

        private static IUISoundService CreateUISoundsService(DIContainer container)
        {
            AudioHub audioHub = container.Resolve<AudioHub>();
            UISoundsConfig config = container.Resolve<ConfigsProviderService>()
                .GetConfig<UISoundsConfig>();

            return new UISoundService(audioHub.UISoundsSource, config);
        }

        private static TimerServiceFactory CreateTimerService(DIContainer container)
            => new TimerServiceFactory(container);

        private static ViewsFactory CreateViewsFactory(DIContainer container)
            => new ViewsFactory(container.Resolve<ResourcesAssetsLoader>());

        private static ProjectPresentersFactory CreateProjectPresentersFactory(DIContainer container)
            => new ProjectPresentersFactory(container);

        private static StatsService CreateStatsService(DIContainer container)
        {
            ReactiveVariable<int> wins = new();
            ReactiveVariable<int> losses = new();

            return new StatsService(wins, losses, container.Resolve<PlayerDataProvider>());
        }

        private static PlayerDataProvider CreatePlayerDataProvider(DIContainer container)
            => new PlayerDataProvider(container.Resolve<ISaveLoadSerivce>(), container.Resolve<ConfigsProviderService>());

        private static SaveLoadService CreateSaveLoadService(DIContainer container)
        {
            IDataSerializer dataSerializer = new JsonSerializer();
            IDataKeysStorage dataKeysStorage = new MapDataKeysStorage();

            string saveFolderPath = Application.isEditor ? Application.dataPath : Application.persistentDataPath;

            IDataRepository dataRepository = new LocalFileDataRepository(saveFolderPath, "json");

            return new SaveLoadService(dataSerializer, dataKeysStorage, dataRepository);
        }

        private static WalletService CreateWalletService(DIContainer container)
        {
            Dictionary<CurrencyTypes, ReactiveVariable<int>> currencies = new();

            foreach (CurrencyTypes currencyType in Enum.GetValues(typeof(CurrencyTypes)))
            {
                if (currencyType == CurrencyTypes.Essence)
                    continue;

                currencies[currencyType] = new ReactiveVariable<int>();
            }

            return new WalletService(currencies, container.Resolve<PlayerDataProvider>());
        }

        private static PersistedGoldRewardService CreatePersistedGoldRewardService(DIContainer container)
        {
            return new PersistedGoldRewardService(
                container.Resolve<WalletService>(),
                container.Resolve<PlayerDataProvider>(),
                container.Resolve<ICoroutinesPerformer>());
        }

        private static SceneSwitcherService CreateSceneSwitcherService(DIContainer container)
            => new SceneSwitcherService(
                container.Resolve<SceneLoaderService>(),
                container.Resolve<ILoadingScreen>(),
                container);

        private static SceneLoaderService CreateSceneLoaderService(DIContainer container)
            => new SceneLoaderService();

        private static ConfigsProviderService CreateConfigsProviderService(DIContainer container)
        {
            ResourcesAssetsLoader resourcesAssetsLoader = container.Resolve<ResourcesAssetsLoader>();
            ResourcesConfigsLoader resourcesConfigsLoader = new ResourcesConfigsLoader(resourcesAssetsLoader);
            return new ConfigsProviderService(resourcesConfigsLoader);
        }

        private static ResourcesAssetsLoader CreateResourcesAssetsLoader(DIContainer container)
            => new ResourcesAssetsLoader();

        private static CoroutinesPerformer CreateCoroutinesPerformer(DIContainer container)
        {
            ResourcesAssetsLoader resourcesAssetsLoader = container.Resolve<ResourcesAssetsLoader>();

            CoroutinesPerformer coroutinesPerformerPrefab = resourcesAssetsLoader
                .Load<CoroutinesPerformer>("Utilities/CoroutinesPerformer");

            return Object.Instantiate(coroutinesPerformerPrefab);
        }

        private static StandardLoadingScreen CreateLoadingScreen(DIContainer container)
        {
            ResourcesAssetsLoader resourcesAssetsLoader = container.Resolve<ResourcesAssetsLoader>();

            StandardLoadingScreen standardLoadingScreenPrefab = resourcesAssetsLoader
                .Load<StandardLoadingScreen>("Utilities/StandardLoadingScreen");

            return Object.Instantiate(standardLoadingScreenPrefab);
        }
    }
}
