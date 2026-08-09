using Assets._Project.Develop.Runtime.Configs.Gameplay.Juice;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.AI;
using Assets._Project.Develop.Runtime.Gameplay.Features.MainHero;
using Assets._Project.Develop.Runtime.Gameplay.Features.PlantBuildingBuff;
using Assets._Project.Develop.Runtime.Gameplay.Features.PlantPlacementFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.EssenceFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.JuiceFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.RunKillCounter;
using Assets._Project.Develop.Runtime.Gameplay.States;
using Assets._Project.Develop.Runtime.Infrastructure;
using Assets._Project.Develop.Runtime.Infrastructure.DI;
using Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature;
using Assets._Project.Develop.Runtime.Utilities.ConfigsManagment;
using Assets._Project.Develop.Runtime.Utilities.SceneManagment;
using System;
using System.Collections;
using _Project.Develop.Runtime.UI.Gameplay;
using Assets._Project.Develop.Runtime.Gameplay.Features.GameplayTimeScale;
using Assets._Project.Develop.Runtime.Gameplay.Features.Ability;
using Assets._Project.Develop.Runtime.Gameplay.Features.SpellcoreProgressionFeature;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Infrastructure
{
    public class GameplayBootstrap : SceneBootstrap
    {
        private const int SIMULATE_COMPLETED_WAVES = 4;
        private const int SIMULATE_ESSENCE_AMOUNT = 2000;

        private DIContainer _container;
        private GameplayInputArgs _inputArgs;

        private GameplayStatesContext _gameplayStatesContext;
        private EntitiesLifeContext _entitiesLifeContext;
        private AIBrainsContext _brainsContext;
        
        private GameplayScreenPresenter _screenPresenter;
        private EssenceFeatureService _essenceFeatureService;
        private RunEnemyKillCounterService _runEnemyKillCounterService;
        private GameplayTimeScaleService _gameplayTimeScaleService;

        public override void ProcessRegistrations(DIContainer container, IInputSceneArgs sceneArgs = null)
        {
            _container = container;

            if (sceneArgs is not GameplayInputArgs gameplayInputArgs)
                throw new ArgumentException($"{nameof(sceneArgs)} is not matching with {typeof(GameplayInputArgs)} type");

            _inputArgs = gameplayInputArgs;

            GameplayContextRegistrations.Process(_container, _inputArgs);
        }

        public override IEnumerator Initialize()
        {
            Debug.Log($"Вы попали на уровень {_inputArgs.LevelNumber}");
            Debug.Log("Инициализация геймплейной сцены");

            _entitiesLifeContext = _container.Resolve<EntitiesLifeContext>();
            _brainsContext = _container.Resolve<AIBrainsContext>();
            _gameplayStatesContext = _container.Resolve<GameplayStatesContext>();
            
            _container.Resolve<SectorGridFactory>().Create();
            GameplayVfxUtility.Configure(_container.Resolve<ConfigsProviderService>().GetConfig<GameplayVfxConfig>());
            _container.Resolve<PlantPlacementService>().ClearForNewRun();
            _container.Resolve<PlantBuildingBuffService>().ClearForNewRun();
            _container.Resolve<SpellcoreProgressionService>().InitializeForRun();
            _container.Resolve<SpellcoreCoachToastService>().InitializeForRun();

            _essenceFeatureService = _container.Resolve<EssenceFeatureService>();
            _essenceFeatureService.InitializeForRun();

            _runEnemyKillCounterService = _container.Resolve<RunEnemyKillCounterService>();
            _runEnemyKillCounterService.InitializeForRun();

            _gameplayTimeScaleService = _container.Resolve<GameplayTimeScaleService>();
            _gameplayTimeScaleService.ResetToNormal();

            _container.Resolve<MainHeroFactory>().Create();
			_container.Resolve<MainHeroFactory>().CreateTowerWalker();
            _container.Resolve<MainHeroFactory>().CreateTowerBrother();

            TryApplySimulatedFourWaveCompletion();
            
            _screenPresenter = _container.Resolve<GameplayScreenPresenter>();
            _screenPresenter.Initialize();
            yield break;
        }

        public override void Run()
        {
            Debug.Log("Старт геймплейной сцены");

            _gameplayStatesContext.Run();
        }

        private void Update()
        {
            _entitiesLifeContext?.Update(Time.deltaTime);
            _brainsContext?.Update(Time.deltaTime);
            _gameplayStatesContext?.Update(Time.deltaTime);
            _essenceFeatureService?.Update(Time.deltaTime);
        }
        
        private void LateUpdate()
        {
            _screenPresenter?.LateUpdate();
        }

        private void OnDestroy()
        {
            _gameplayTimeScaleService?.ResetToNormal();
        }

        private void TryApplySimulatedFourWaveCompletion()
        {
            if (GameplayEditorDebugPrefs.IsSimulateFourWaveCompletionEnabled == false)
                return;

            SpellcoreProgressionService spellcoreProgressionService =
                _container.Resolve<SpellcoreProgressionService>();
            StageProviderService stageProviderService = _container.Resolve<StageProviderService>();
            RunEssenceService runEssenceService = _container.Resolve<RunEssenceService>();

            spellcoreProgressionService.ApplyDebugSimulatedCompletedWaves(SIMULATE_COMPLETED_WAVES);
            stageProviderService.ApplyDebugSimulatedCompletedWaves(SIMULATE_COMPLETED_WAVES);
            runEssenceService.Add(SIMULATE_ESSENCE_AMOUNT);

            Debug.Log(
                $"Debug: simulated {SIMULATE_COMPLETED_WAVES} completed waves, prep before wave 5, +{SIMULATE_ESSENCE_AMOUNT} essence.");
        }
    }
}
