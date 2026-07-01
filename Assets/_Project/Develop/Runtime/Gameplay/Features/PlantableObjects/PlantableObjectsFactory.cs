using System;

using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;

using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;

using Assets._Project.Develop.Runtime.Gameplay.Features.AI;

using Assets._Project.Develop.Runtime.Gameplay.Features.AI.States;

using Assets._Project.Develop.Runtime.Gameplay.Features.PlantCombatFeature;

using Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature;

using Assets._Project.Develop.Runtime.Gameplay.Features.TeamsFeature;

using Assets._Project.Develop.Runtime.Gameplay.Infrastructure;

using Assets._Project.Develop.Runtime.Infrastructure.DI;

using Assets._Project.Develop.Runtime.Utilities.Reactive;

using UnityEngine;



namespace _Project.Develop.Runtime.Gameplay.Features.PlantableObjects

{

    public class PlantableObjectsFactory

    {

        private readonly DIContainer _container;



        private readonly EntitiesFactory _entitiesFactory;

        private readonly BrainsFactory _brainsFactory;

        private readonly EntitiesLifeContext _entitiesLifeContext;

        private readonly SectorEnemyQueryService _sectorEnemyQueryService;



        public PlantableObjectsFactory(DIContainer container)

        {

            _container = container;

            _entitiesFactory = _container.Resolve<EntitiesFactory>();

            _brainsFactory = _container.Resolve<BrainsFactory>();

            _entitiesLifeContext = _container.Resolve<EntitiesLifeContext>();

            _sectorEnemyQueryService = _container.Resolve<SectorEnemyQueryService>();

        }



        public Entity Create(Vector3 position, EntityConfig entityConfig, SectorId plantSector, int plantedEssenceCost)

        {

            Entity entity;



            switch (entityConfig)

            {

                case TurretConfig turretConfig:

                    entity = _entitiesFactory.CreateTurret(position, turretConfig);



                    _brainsFactory.CreateRotatingAutoAttackBrain(

                        entity,

                        new TurretPathTargetSelector(entity, _sectorEnemyQueryService));



                    break;



                case MineConfig mineConfig:

                    entity = _entitiesFactory.CreateMine(position, mineConfig);

                    break;

                

                case ToxicAreaConfig toxicAreaConfig:

                    entity = _entitiesFactory.CreateToxicArea(position, toxicAreaConfig);

                    break;



                default:

                    throw new ArgumentException($"{entityConfig.GetType()} config type is not supported");

            }



            entity

                .AddTeam(new ReactiveVariable<Teams>(Teams.MainHero))

                .AddPlantSector(plantSector)

                .AddPlantedEssenceCost(plantedEssenceCost);



            _entitiesLifeContext.Add(entity);



            return entity;

        }

    }

}

