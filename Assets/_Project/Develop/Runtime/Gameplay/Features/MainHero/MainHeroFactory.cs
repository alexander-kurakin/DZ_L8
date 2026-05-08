using _Project.Develop.Runtime.Meta.Features.Powerups;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Levels;
using Assets._Project.Develop.Runtime.Configs.Meta.NewPowerups;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.Ability;
using Assets._Project.Develop.Runtime.Gameplay.Features.AI;
using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.TeamsFeature;
using Assets._Project.Develop.Runtime.Infrastructure.DI;
using Assets._Project.Develop.Runtime.Meta.Features.Powerups;
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
        private readonly PowerupConfigsContainer  _powerupConfigsContainer;
        private readonly PowerupService _powerupService;
        private readonly PowerupFactory _powerupFactory;
        
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
            _powerupConfigsContainer = _configsProviderService.GetConfig<PowerupConfigsContainer>();
            _powerupService = _container.Resolve<PowerupService>();
            _powerupFactory = _container.Resolve<PowerupFactory>();
            
            _currentLevelNumber =  currentLevelNumber;
        }

        public Entity Create()
        {
            TowerConfig towerConfig = _configsProviderService.GetConfig<TowerConfig>();
            LevelConfig levelConfig = _configsProviderService.GetConfig<LevelsListConfig>().GetBy(_currentLevelNumber);

            Entity entity = _entitiesFactory.CreateTower(towerConfig, levelConfig);

            entity
                .AddIsMainHero()
                .AddGameplayPhase()
                .AddTeam(new ReactiveVariable<Teams>(Teams.MainHero))
                .AddAbilityUserActiveAbility()
                .AddAbilityUserAllAbilities()
                .AddAbilityUserPlantAbilityPreference();

            entity
                .AddPowerup()
                .AddSystem(new PowerupOnAddActivatorSystem());
            
            ApplyPermanentPowerups(entity);
            _abilitiesFactory.SetupAbilitiesForMainHero(entity);
            
            _entitiesLifeContext.Add(entity);
            
            _townWalkerSpawnPoint = entity.SpawnPoint;

            return entity;
        }

        private void ApplyPermanentPowerups(Entity mainHero)
        {
            foreach (PowerupConfig powerupConfig in _powerupConfigsContainer.PowerupConfigs)
            {
                PowerupSaveData powerupSaveData = _powerupService.GetPowerupDataByID(powerupConfig.ID);

                if (powerupSaveData.Unlocked == false)
                    continue;

                Powerup powerup = _powerupFactory.CreatePowerupFor(mainHero, powerupConfig, powerupSaveData.Level);
                mainHero.Powerup.Add(powerup);
            }
        }

        public Entity CreateTowerWalker()
        {
            
			Entity entity = _entitiesFactory.CreateTowerWalker(_townWalkerSpawnPoint.position);
            
            
            entity
                .AddGameplayPhase()
                .AddTeam(new ReactiveVariable<Teams>(Teams.MainHero));

            _mainHeroHolderService.RegisterTowerWalker(entity);

            _entitiesLifeContext.Add(entity);
            _brainsFactory.CreateTowerWalkerBrain(entity, _mouseInput);
            
            return entity;
        }
    }
}
