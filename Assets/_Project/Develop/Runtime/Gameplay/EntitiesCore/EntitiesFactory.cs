using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using Assets._Project.Develop.Runtime.Gameplay.Features.MainHero;
using Assets._Project.Develop.Runtime.Infrastructure.DI;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.EntitiesCore
{
    public class EntitiesFactory
    {
        private readonly DIContainer _container;
        private readonly EntitiesLifeContext _entitiesLifeContext;
        private readonly CollidersRegistryService _collidersRegistryService;
        private readonly MonoEntitiesFactory _monoEntitiesFactory;
        private readonly MainHeroHolderService _mainHeroHolderService;
        
        public EntitiesFactory(DIContainer container)
        {
            _container = container;
            _entitiesLifeContext = _container.Resolve<EntitiesLifeContext>();
            _monoEntitiesFactory = _container.Resolve<MonoEntitiesFactory>();
            _collidersRegistryService = _container.Resolve<CollidersRegistryService>();
            _mainHeroHolderService = _container.Resolve<MainHeroHolderService>();
        }
        
        public Entity CreateMainHero(HeroConfig config)
        {
            Entity entity = CreateEmpty();
            Vector3 startPosition = config.StartPosition;
            
            _monoEntitiesFactory.Create(entity, startPosition, config.PrefabPath);

            return entity;
        }

        public Entity CreateProjectile(Vector3 position, Entity owner)
        {
            Entity entity = CreateEmpty();

            _monoEntitiesFactory.Create(entity, position, "Entities/Projectile");

            entity
                .AddProjectileOwner(owner);

            return entity;
        }

        public Entity CreateEmpty() => new Entity();
    }
}
