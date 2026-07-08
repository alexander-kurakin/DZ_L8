using _Project.Develop.Runtime.UI.Gameplay;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.RunKillCounter;
using Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.WaveProgressFeature;
using Assets._Project.Develop.Runtime.Gameplay.States;
using Assets._Project.Develop.Runtime.Infrastructure;
using Assets._Project.Develop.Runtime.Infrastructure.DI;
using Assets._Project.Develop.Runtime.Utilities.SceneManagment;
using System;
using System.Collections;
using Assets._Project.Develop.Runtime.Gameplay.Features.AI;
using Assets._Project.Develop.Runtime.Gameplay.Features.MainHero;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Infrastructure
{
    public class GameplayBootstrap : SceneBootstrap
    {
        private DIContainer _container;
        private GameplayInputArgs _inputArgs;

        private GameplayStatesContext _gameplayStatesContext;
        private EntitiesLifeContext _entitiesLifeContext;
        private AIBrainsContext _aiBrainsContext;
        private GameplayScreenPresenter _screenPresenter;
        private RunEnemyKillCounterService _runEnemyKillCounterService;
        private WaveProgressService _waveProgressService;

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
            Debug.Log($"You have entered level {_inputArgs.LevelNumber}");
            Debug.Log("Initializing gameplay scene");

            _entitiesLifeContext = _container.Resolve<EntitiesLifeContext>();
            _gameplayStatesContext = _container.Resolve<GameplayStatesContext>();
            _aiBrainsContext =  _container.Resolve<AIBrainsContext>();

            _waveProgressService = _container.Resolve<WaveProgressService>();
            _waveProgressService.InitializeForRun();

            _runEnemyKillCounterService = _container.Resolve<RunEnemyKillCounterService>();
            _runEnemyKillCounterService.InitializeForRun();
            
            _container.Resolve<MainHeroFactory>().Create();
            _container.Resolve<PlayerModifiersHolderService>().Create();

            _screenPresenter = _container.Resolve<GameplayScreenPresenter>();
            _screenPresenter.Initialize();
            yield break;
        }

        public override void Run()
        {
            Debug.Log("Starting gameplay scene");
            _gameplayStatesContext.Run();
        }

        private void Update()
        {
            _entitiesLifeContext?.Update(Time.deltaTime);
            _gameplayStatesContext?.Update(Time.deltaTime);
            _aiBrainsContext?.Update(Time.deltaTime);
        }

        private void OnDestroy()
        {
            _waveProgressService?.Dispose();
        }
    }
}
