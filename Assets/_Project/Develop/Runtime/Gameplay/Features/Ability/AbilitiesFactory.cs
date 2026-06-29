using System.Collections.Generic;
using _Project.Develop.Runtime.Gameplay.Features.AbilitySystems;
using _Project.Develop.Runtime.Gameplay.Features.DealAreaDamage;
using _Project.Develop.Runtime.Gameplay.Features.PlantableObjects;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.Ability;
using Assets._Project.Develop.Runtime.Gameplay.Features.PlantPlacementFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.SpellcoreProgressionFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.TeamsFeature;
using Assets._Project.Develop.Runtime.Infrastructure.DI;
using Assets._Project.Develop.Runtime.Gameplay.Features.EssenceFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.JuiceFeature;
using Assets._Project.Develop.Runtime.Utilities.ConfigsManagment;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using _Project.Develop.Runtime.Gameplay.Features.ExplosionAbilityPreview;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Ability
{
    public class AbilitiesFactory
    {
        private readonly EntitiesLifeContext _entitiesLifeContext;
        private readonly RunEssenceService _runEssenceService;
        private readonly ConfigsProviderService _configsProviderService;
        private readonly StageProviderService _stageProviderService;
        private readonly PlantableObjectsFactory _plantableObjectsFactory;
        private readonly SpellcoreProgressionService _spellcoreProgressionService;
        private readonly PlantPlacementService _plantPlacementService;
        private readonly SectorMembershipService _sectorMembershipService;
        private readonly SectorEnemyQueryService _sectorEnemyQueryService;
        private readonly SectorRegistryService _sectorRegistryService;
        private readonly LmbFlavorToastService _lmbFlavorToastService;
        private readonly LmbFrostProjectileService _lmbFrostProjectileService;
        private readonly GameplayJuiceService _gameplayJuiceService;
        
        private ExplodeAtPointAbilityConfig _explodeAtPointAbilityConfig;
        
        public AbilitiesFactory(DIContainer container)
        {
            _entitiesLifeContext = container.Resolve<EntitiesLifeContext>();
            _runEssenceService = container.Resolve<RunEssenceService>();
            _configsProviderService = container.Resolve<ConfigsProviderService>();
            _stageProviderService = container.Resolve<StageProviderService>();
            _plantableObjectsFactory = container.Resolve<PlantableObjectsFactory>();
            _spellcoreProgressionService = container.Resolve<SpellcoreProgressionService>();
            _plantPlacementService = container.Resolve<PlantPlacementService>();
            _sectorMembershipService = container.Resolve<SectorMembershipService>();
            _sectorEnemyQueryService = container.Resolve<SectorEnemyQueryService>();
            _sectorRegistryService = container.Resolve<SectorRegistryService>();
            _lmbFlavorToastService = container.Resolve<LmbFlavorToastService>();
            _lmbFrostProjectileService = container.Resolve<LmbFrostProjectileService>();
            _gameplayJuiceService = container.Resolve<GameplayJuiceService>();
            
            _explodeAtPointAbilityConfig = _configsProviderService.GetConfig<ExplodeAtPointAbilityConfig>();
        }

        public void SetupAbilitiesForMainHero(Entity mainHero)
        {
            Dictionary<AbilityType, Entity> mapping = mainHero.AbilityUserAllAbilities;

            Entity plantMineAbility = CreatePlantMineAbility(
                mainHero, _configsProviderService.GetConfig<MineConfig>());
            
            Entity plantTurretAbility = CreatePlantTurretAbility(
                mainHero, _configsProviderService.GetConfig<TurretConfig>());
            
            Entity plantToxicAreaAbility = CreatePlantToxicAreaAbility(
                mainHero, _configsProviderService.GetConfig<ToxicAreaConfig>());
            
            Entity explodeAtPointAbility = CreateExplodeAtPointAbility(mainHero);
            
            mapping[AbilityType.PlantMine] = plantMineAbility;
            mapping[AbilityType.PlantTurret] = plantTurretAbility;
            mapping[AbilityType.PlantToxicArea] = plantToxicAreaAbility;
            mapping[AbilityType.ExplodeAtPoint] = explodeAtPointAbility;

            mainHero.AbilityUserPlantAbilityPreference.Value = AbilityType.PlantMine;
            
            //only register in life context when abilities is assigned to someone, e.g. here owner is mainHero
            _entitiesLifeContext.Add(plantMineAbility);
            _entitiesLifeContext.Add(plantTurretAbility); 
            _entitiesLifeContext.Add(plantToxicAreaAbility); 
            _entitiesLifeContext.Add(explodeAtPointAbility);
        }
        
        private Entity CreatePlantMineAbility(
            Entity abilityOwner,
            PurchasableEntityConfig purchasableEntityConfig)
        {
            Entity entity = CreateEmpty();
            
            Teams ownerTeam = abilityOwner.Team.Value;
            
            entity
                .AddAbilityOwner(new ReactiveVariable<Entity>(abilityOwner))
                .AddTeam(new ReactiveVariable<Teams>(ownerTeam))
                .AddAbilityTypeName(new ReactiveVariable<AbilityType>(AbilityType.PlantMine))
                .AddAbilityUseRequest();

            entity
                .AddSystem(new PlantMineSystem(
                    _runEssenceService, 
                    _plantableObjectsFactory,
                    purchasableEntityConfig,
                    _spellcoreProgressionService,
                    _plantPlacementService));

            return entity;
        }
        
        private Entity CreatePlantTurretAbility(
            Entity abilityOwner,
            PurchasableEntityConfig purchasableEntityConfig)
        {
            Entity entity = CreateEmpty();
            
            Teams ownerTeam = abilityOwner.Team.Value;
            
            entity
                .AddAbilityOwner(new ReactiveVariable<Entity>(abilityOwner))
                .AddTeam(new ReactiveVariable<Teams>(ownerTeam))
                .AddAbilityTypeName(new ReactiveVariable<AbilityType>(AbilityType.PlantTurret))
                .AddAbilityUseRequest();

            entity
                .AddSystem(new PlantTurretSystem(
                    _runEssenceService, 
                    _plantableObjectsFactory,
                    purchasableEntityConfig,
                    _spellcoreProgressionService,
                    _plantPlacementService));

            return entity;
        }
        
        private Entity CreatePlantToxicAreaAbility(
            Entity abilityOwner,
            PurchasableEntityConfig purchasableEntityConfig)
        {
            Entity entity = CreateEmpty();
            
            Teams ownerTeam = abilityOwner.Team.Value;
            
            entity
                .AddAbilityOwner(new ReactiveVariable<Entity>(abilityOwner))
                .AddTeam(new ReactiveVariable<Teams>(ownerTeam))
                .AddAbilityTypeName(new ReactiveVariable<AbilityType>(AbilityType.PlantToxicArea))
                .AddAbilityUseRequest();

            entity
                .AddSystem(new PlantToxicAreaSystem(
                    _runEssenceService, 
                    _plantableObjectsFactory,
                    purchasableEntityConfig,
                    _stageProviderService,
                    _spellcoreProgressionService,
                    _plantPlacementService));

            return entity;
        }

        private Entity CreateExplodeAtPointAbility(Entity abilityOwner)
        {
            Entity entity = CreateEmpty();

            Teams ownerTeam = abilityOwner.Team.Value;
            
            entity
                .AddAbilityOwner(new ReactiveVariable<Entity>(abilityOwner))
                .AddAbilityTypeName(new ReactiveVariable<AbilityType>(AbilityType.ExplodeAtPoint))
                .AddAbilityUseRequest()
                .AddTeam(new ReactiveVariable<Teams>(ownerTeam))
                .AddDealAreaImpactDamageRequest();

            entity
                .AddSystem(new ExplodeAtPointSystem(
                    _sectorMembershipService,
                    _sectorEnemyQueryService,
                    _sectorRegistryService,
                    _lmbFlavorToastService,
                    _explodeAtPointAbilityConfig,
                    _lmbFrostProjectileService,
                    _gameplayJuiceService));
            
            return entity;
        }

        private Entity CreateEmpty() => new Entity();
    }
}