using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.AI;
using Assets._Project.Develop.Runtime.Gameplay.Features.TeamsFeature;
using Assets._Project.Develop.Runtime.Infrastructure.DI;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System;
using Assets._Project.Develop.Runtime.Gameplay.Features.AI.States;
using Assets._Project.Develop.Runtime.Gameplay.Features.MainHero;
using Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Enemies
{
    public class EnemiesFactory
    {
        private readonly DIContainer _container;

        private readonly EntitiesFactory _entitiesFactory;
        private readonly BrainsFactory _brainsFactory;
        private readonly EntitiesLifeContext _entitiesLifeContext;
        private readonly MainHeroHolderService _mainHeroHolderService;
        private readonly SectorMembershipService _sectorMembershipService;

        public EnemiesFactory(DIContainer container)
        {
            _container = container;
            _entitiesFactory = _container.Resolve<EntitiesFactory>();
            _brainsFactory = _container.Resolve<BrainsFactory>();
            _entitiesLifeContext = _container.Resolve<EntitiesLifeContext>();
            _mainHeroHolderService = _container.Resolve<MainHeroHolderService>();
            _sectorMembershipService = _container.Resolve<SectorMembershipService>();
        }

        public Entity Create(Vector3 position, EntityConfig config)
        {
            Entity entity;

            switch (config)
            {
                case RangedDotWalkingEnemyConfig rangedDotWalkingEnemyConfig:
                    entity = _entitiesFactory.CreateRangedDoTWalkingEnemy(position, rangedDotWalkingEnemyConfig);
                    _brainsFactory.CreateWalkingTowardsTargetBrain(entity, new MainHeroTargetSelector(_mainHeroHolderService));
                    break;

                case RangedShootingEnemyConfig rangedShootingEnemyConfig:
                    entity = _entitiesFactory.CreateRangedShootingEnemy(position, rangedShootingEnemyConfig);
                    _brainsFactory.CreateWalkingToRangedAutoAttackBrain(entity, new MainHeroTargetSelector(_mainHeroHolderService));
                    break;
                
                case ExplodingWalkingEnemyConfig explodingWalkingEnemyConfig:
                    entity = _entitiesFactory.CreateExplodingWalkingEnemy(position, explodingWalkingEnemyConfig);
                    _brainsFactory.CreateWalkingTowardsTargetBrain(entity, new MainHeroTargetSelector(_mainHeroHolderService));
                    break;

                default:
                    throw new ArgumentException($"Not support {config.GetType()} type config");
            }

            entity.AddTeam(new ReactiveVariable<Teams>(Teams.Enemies));

            SetupSectorTracking(entity, position);

            _entitiesLifeContext.Add(entity);

            return entity;
        }

        private void SetupSectorTracking(Entity entity, Vector3 spawnPosition)
        {
            SectorId initialSector = _sectorMembershipService.ResolveFromWorldPosition(spawnPosition);

            entity
                .AddCurrentSector(new ReactiveVariable<SectorId>(initialSector))
                .AddSystem(new SectorMembershipSystem(_sectorMembershipService));
        }
    }
}
