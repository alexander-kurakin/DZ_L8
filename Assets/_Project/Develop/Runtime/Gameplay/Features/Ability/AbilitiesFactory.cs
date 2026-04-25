using System.Collections.Generic;
using _Project.Develop.Runtime.Gameplay.Features.AbilitySystems;
using _Project.Develop.Runtime.Gameplay.Features.DealAreaDamage;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.TeamsFeature;
using Assets._Project.Develop.Runtime.Infrastructure.DI;
using Assets._Project.Develop.Runtime.Meta.Features.Wallet;
using Assets._Project.Develop.Runtime.Utilities;
using Assets._Project.Develop.Runtime.Utilities.ConfigsManagment;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Ability
{
    public class AbilitiesFactory
    {
        private readonly EntitiesLifeContext _entitiesLifeContext;
        private readonly EntitiesFactory _entitiesFactory; //mines
        private readonly WalletService _walletService; //mines
        private readonly ConfigsProviderService _configsProviderService;
        private readonly CollidersRegistryService _collidersRegistryService;
        
        private ExplodeAtPointAbilityConfig _explodeAtPointAbilityConfig;
        
        public AbilitiesFactory(DIContainer container)
        {
            _entitiesLifeContext = container.Resolve<EntitiesLifeContext>();
            _entitiesFactory = container.Resolve<EntitiesFactory>();
            _walletService = container.Resolve<WalletService>();
            _configsProviderService = container.Resolve<ConfigsProviderService>();
            _collidersRegistryService = container.Resolve<CollidersRegistryService>();
            
            _explodeAtPointAbilityConfig = _configsProviderService.GetConfig<ExplodeAtPointAbilityConfig>();
        }

        public void SetupAbilitiesForMainHero(Entity mainHero)
        {
            Dictionary<AbilityType, Entity> mapping = mainHero.AbilityUserAllAbilities;

            Entity plantMineAbility = CreatePlantableObjectAbility(mainHero, AbilityType.PlantMine, _configsProviderService.GetConfig<MineConfig>());
            Entity plantTurretAbility = CreatePlantableObjectAbility(mainHero, AbilityType.PlantTurret, _configsProviderService.GetConfig<TurretConfig>());
            Entity plantToxicAreaAbility = CreatePlantableObjectAbility(mainHero, AbilityType.PlantToxicArea, _configsProviderService.GetConfig<ToxicAreaConfig>());
            Entity explodeAtPointAbility = CreateExplodeAtPointAbility(mainHero);
            
            mapping[AbilityType.PlantMine] = plantMineAbility;
            mapping[AbilityType.PlantTurret] = plantTurretAbility;
            mapping[AbilityType.PlantToxicArea] = plantToxicAreaAbility;
            mapping[AbilityType.ExplodeAtPoint] = explodeAtPointAbility;
            
            //only register in life context when abilities is assigned to someone, e.g. here owner is mainHero
            _entitiesLifeContext.Add(plantMineAbility);
            _entitiesLifeContext.Add(plantTurretAbility); 
            _entitiesLifeContext.Add(plantToxicAreaAbility); 
            _entitiesLifeContext.Add(explodeAtPointAbility);
        }
        
        private Entity CreatePlantableObjectAbility(
            Entity abilityOwner,
            AbilityType abilityType,
            PurchasableEntityConfig purchasableEntityConfig)
        {
            Entity entity = CreateEmpty();
            
            Teams ownerTeam = Teams.MainHero; //default
            
            if (abilityOwner.TryGetTeam(out ReactiveVariable<Teams>team))
                ownerTeam = team.Value;
            
            entity
                .AddAbilityOwner(new ReactiveVariable<Entity>(abilityOwner))
                .AddTeam(new ReactiveVariable<Teams>(ownerTeam))
                .AddAbilityTypeName(new ReactiveVariable<AbilityType>(abilityType))
                .AddAbilityUseRequest();

            entity
                .AddSystem(new PlantPurchasedObjectsSystem(
                    _walletService, 
                    _entitiesFactory,
                    purchasableEntityConfig));

            return entity;
        }
        
        private Entity CreateExplodeAtPointAbility(Entity abilityOwner)
        {
            Entity entity = CreateEmpty();

            Teams ownerTeam = Teams.MainHero; //default
            
            if (abilityOwner.TryGetTeam(out ReactiveVariable<Teams>team))
                ownerTeam = team.Value;
            
            entity
                .AddAbilityOwner(new ReactiveVariable<Entity>(abilityOwner))
                .AddAbilityTypeName(new ReactiveVariable<AbilityType>(AbilityType.ExplodeAtPoint))
                .AddAbilityUseRequest()
                .AddTeam(new ReactiveVariable<Teams>(ownerTeam))
                .AddAreaImpactDamage(new ReactiveVariable<float>(_explodeAtPointAbilityConfig.ExplosionDamage))
                .AddAreaImpactRadius(new ReactiveVariable<float>(_explodeAtPointAbilityConfig.ExplosionRadius))
                .AddAreaImpactMask(Layers.CharactersMask)
                .AddAreaImpactCollidersBuffer(new Buffer<Collider>(64))
                .AddAreaImpactEntitiesBuffer(new Buffer<Entity>(64))
                .AddDealAreaImpactDamageRequest();

            entity
                .AddSystem(new ExplodeAtPointSystem())
                .AddSystem(new AreaDamageContactDetectingSystem(_collidersRegistryService))
                .AddSystem(new AreaDamageEntitiesFilterSystem(_collidersRegistryService))
                .AddSystem(new DealAreaDamageSystem());
            
            return entity;
        }

        private Entity CreateEmpty() => new Entity();
    }
}