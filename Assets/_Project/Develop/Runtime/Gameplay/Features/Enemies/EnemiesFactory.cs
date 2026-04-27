using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.AI;
using Assets._Project.Develop.Runtime.Gameplay.Features.TeamsFeature;
using Assets._Project.Develop.Runtime.Infrastructure.DI;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System;
using Assets._Project.Develop.Runtime.Gameplay.Features.AI.States;
using Assets._Project.Develop.Runtime.Gameplay.Features.MainHero;
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

        public EnemiesFactory(DIContainer container)
        {
            _container = container;
            _entitiesFactory = _container.Resolve<EntitiesFactory>();
            _brainsFactory = _container.Resolve<BrainsFactory>();
            _entitiesLifeContext = _container.Resolve<EntitiesLifeContext>();
            _mainHeroHolderService = _container.Resolve<MainHeroHolderService>();
        }

        public Entity Create(Vector3 position, EntityConfig config)
        {
            Entity entity;

            switch (config)
            {
                case WalkingEnemyConfig walkingEnemyConfig:
                    entity = _entitiesFactory.CreateWalkingEnemy(position, walkingEnemyConfig);
                    _brainsFactory.CreateWalkingEnemyBrain(entity, new MainHeroTargetSelector(_mainHeroHolderService));
                    break;
                
                case RangedEnemyConfig rangedEnemyConfig:
                    entity = _entitiesFactory.CreateRangedEnemy(position, rangedEnemyConfig);
                    _brainsFactory.CreateRangedEnemyBrain(entity, new MainHeroTargetSelector(_mainHeroHolderService));
                    break;

                default:
                    throw new ArgumentException($"Not support {config.GetType()} type config");
            }

            entity.AddTeam(new ReactiveVariable<Teams>(Teams.Enemies));

            _entitiesLifeContext.Add(entity);

            return entity;
        }
    }
}
