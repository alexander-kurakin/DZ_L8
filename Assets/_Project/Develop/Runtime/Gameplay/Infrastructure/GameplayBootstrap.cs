using Assets._Project.Develop.Runtime.Configs.Gameplay.Juice;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.AI;
using Assets._Project.Develop.Runtime.Gameplay.Features.MainHero;
using Assets._Project.Develop.Runtime.Gameplay.Features.PlantPlacementFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.EssenceFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.JuiceFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.SpellcoreProgressionFeature;
using Assets._Project.Develop.Runtime.Gameplay.States;
using Assets._Project.Develop.Runtime.Infrastructure;
using Assets._Project.Develop.Runtime.Infrastructure.DI;
using Assets._Project.Develop.Runtime.Utilities.ConfigsManagment;
using Assets._Project.Develop.Runtime.Utilities.SceneManagment;
using System;
using System.Collections;
using _Project.Develop.Runtime.UI.Gameplay;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Infrastructure
{
    public class GameplayBootstrap : SceneBootstrap
    {
        private DIContainer _container;
        private GameplayInputArgs _inputArgs;

        private GameplayStatesContext _gameplayStatesContext;
        private EntitiesLifeContext _entitiesLifeContext;
        private AIBrainsContext _brainsContext;
        
        private GameplayScreenPresenter _screenPresenter;
        private EssenceFeatureService _essenceFeatureService;

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
            _container.Resolve<SpellcoreProgressionService>().InitializeForRun();

            _essenceFeatureService = _container.Resolve<EssenceFeatureService>();
            _essenceFeatureService.InitializeForRun();

            _container.Resolve<MainHeroFactory>().Create();
			_container.Resolve<MainHeroFactory>().CreateTowerWalker();
            _container.Resolve<MainHeroFactory>().CreateTowerBrother();
            
            _screenPresenter = _container.Resolve<GameplayScreenPresenter>();
            yield break;
        }

        public override void Run()
        {
            Debug.Log("Старт геймплейной сцены");

            _gameplayStatesContext.Run();
        }

        private void Update()
        {
            _brainsContext?.Update(Time.deltaTime);
            _entitiesLifeContext?.Update(Time.deltaTime);
            _gameplayStatesContext?.Update(Time.deltaTime);
            _essenceFeatureService?.Update(Time.deltaTime);
        }
        
        private void LateUpdate()
        {
            _screenPresenter?.LateUpdate();
        }
    }
}
