using _Project.Develop.Runtime.Gameplay.Features.ExplosionAbilityPreview;
using _Project.Develop.Runtime.Gameplay.Features.Input;
using _Project.Develop.Runtime.Gameplay.Features.InputFeature;
using _Project.Develop.Runtime.Meta.Features.Powerups;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Levels;
using Assets._Project.Develop.Runtime.Configs.Gameplay.MouseConfig;
using Assets._Project.Develop.Runtime.Configs.Meta.NewPowerups;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.Ability;
using Assets._Project.Develop.Runtime.Gameplay.Features.AI;
using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.PlantPlacementFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.TeamsFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.TowerWalker;
using Assets._Project.Develop.Runtime.Infrastructure.DI;
using Assets._Project.Develop.Runtime.Meta.Features.Powerups;
using Assets._Project.Develop.Runtime.Utilities.ConfigsManagment;
using Assets._Project.Develop.Runtime.Utilities.Conditions;
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
		private readonly IMouseInputService _mouseInput;
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
			_mouseInput = _container.Resolve<IMouseInputService>();
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
                .AddAbilityUserPlantAbilityPreference()
                .AddExplosionPreviewWorldPoint()
                .AddExplosionPreviewVisible()
                .AddExplosionPreviewIndicatorDiameter()
                .AddExplosionPreviewCooldownFill();

            entity
                .AddPowerup()
                .AddSystem(new PowerupOnAddActivatorSystem())
                .AddSystem(new ExplosionAbilityPreviewSystem(
                    _mouseInput,
                    _container.Resolve<MouseRaycastService>(),
                    _container.Resolve<SectorRegistryService>(),
                    _configsProviderService.GetConfig<ExplodeAtPointAbilityConfig>()))
                .AddSystem(new PlantPlacementPreviewHoverSystem(
                    _mouseInput,
                    _container.Resolve<MouseRaycastService>(),
                    _container.Resolve<MouseOverUIService>(),
                    _container.Resolve<SectorRegistryService>(),
                    _container.Resolve<SectorMembershipService>(),
                    _container.Resolve<PlantPlacementService>(),
                    _container.Resolve<PlantPlacementPreviewService>()));
            
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
            _brainsFactory.CreateWalkingTowardsCursorBrain(entity, _mouseInput);
            
            return entity;
        }
        
        public Entity CreateTowerBrother()
        {
            TowerBrotherStoneThrowConfig stoneThrowConfig =
                _configsProviderService.GetConfig<TowerBrotherStoneThrowConfig>();

            Entity entity = _entitiesFactory.CreateTowerBrother(_townWalkerSpawnPoint.position + (Vector3.right * 2));

            ReactiveVariable<bool> isStoneThrowing = new ReactiveVariable<bool>(false);

            entity
                .AddGameplayPhase()
                .AddTeam(new ReactiveVariable<Teams>(Teams.MainHero))
                .AddBrotherStoneThrowEvent()
                .AddBrotherStoneThrowing(isStoneThrowing)
                .AddSystem(new TowerBrotherStoneThrowSystem(
                    _container.Resolve<SectorEnemyQueryService>(),
                    _entitiesFactory,
                    stoneThrowConfig,
                    _brainsFactory));

            entity.CanMove.Add(new FuncCondition(() => isStoneThrowing.Value == false));
            entity.CanRotate.Add(new FuncCondition(() => isStoneThrowing.Value == false));

            _mainHeroHolderService.RegisterTowerBrother(entity);

            _entitiesLifeContext.Add(entity);
            _brainsFactory.CreateSimpleRandomWalkerBrain(entity);

            return entity;
        }
    }
}
