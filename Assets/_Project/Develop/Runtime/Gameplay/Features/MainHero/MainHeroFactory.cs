using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Levels;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.Ability;
using Assets._Project.Develop.Runtime.Gameplay.Features.AI;
using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.TeamsFeature;
using Assets._Project.Develop.Runtime.Infrastructure.DI;
using Assets._Project.Develop.Runtime.Utilities.ConfigsManagment;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.MainHero
{
    public class MainHeroFactory
    {
        private readonly DIContainer _container;

        private readonly EntitiesFactory _entitiesFactory;
        private readonly AbilitiesFactory _abilitiesFactory;
        private readonly BrainsFactory _brainsFactory; //sorry bro no brain for Main Hero, maybe one day ;D
        private readonly ConfigsProviderService _configsProviderService;
        private readonly EntitiesLifeContext _entitiesLifeContext;
        private readonly MainHeroHolderService _mainHeroHolderService;
		private readonly MouseInput _mouseInput;
        private Transform _townWalkerSpawnPoint;
        private int _currentLevelNumber;

        public MainHeroFactory(DIContainer container, int currentLevelNumber)
        {
            _container = container;
            _entitiesFactory = _container.Resolve<EntitiesFactory>();
            _brainsFactory = _container.Resolve<BrainsFactory>();
            _configsProviderService = _container.Resolve<ConfigsProviderService>();
            _entitiesLifeContext = _container.Resolve<EntitiesLifeContext>();
            _abilitiesFactory =  _container.Resolve<AbilitiesFactory>();
            _mainHeroHolderService = _container.Resolve<MainHeroHolderService>();
			_mouseInput = _container.Resolve<MouseInput>();
            _currentLevelNumber =  currentLevelNumber;
        }

        public Entity Create()
        {
            TowerConfig towerConfig = _configsProviderService.GetConfig<TowerConfig>();
            LevelConfig levelConfig = _configsProviderService.GetConfig<LevelsListConfig>().GetBy(_currentLevelNumber);

            Entity entity = _entitiesFactory.CreateTower(towerConfig, levelConfig);

            entity
                .AddIsMainHero()
                .AddTeam(new ReactiveVariable<Teams>(Teams.MainHero))
                .AddAbilityUserActiveAbility()
                .AddAbilityUserAllAbilities();

            _entitiesLifeContext.Add(entity);
            
			_abilitiesFactory.SetupAbilitiesForMainHero(entity);
			
            _townWalkerSpawnPoint = entity.SpawnPoint;

            return entity;
        }
        
        public Entity CreateTowerWalker()
        {
            
			Entity entity = _entitiesFactory.CreateTowerWalker(_townWalkerSpawnPoint.position);
            
            
            entity
                .AddTeam(new ReactiveVariable<Teams>(Teams.MainHero));

            _mainHeroHolderService.RegisterTowerWalker(entity);

            _entitiesLifeContext.Add(entity);
            _brainsFactory.CreateTowerWalkerBrain(entity, _mouseInput);
            
            return entity;
        }
    }
}
