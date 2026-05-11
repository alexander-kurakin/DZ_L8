using Assets._Project.Develop.Runtime.Infrastructure.DI;
using Assets._Project.Develop.Runtime.Utilities.AssetsManagment;
using System;
using System.Collections.Generic;
using Assets._Project.Develop.Runtime.Utilities.Audio;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono
{
    public class MonoEntitiesFactory : IInitializable, IDisposable
    {
        private readonly ResourcesAssetsLoader _resources;

        private readonly EntitiesLifeContext _entitiesLifeContext;

        private readonly CollidersRegistryService _collidersRegistryService;
        private readonly IGameSoundsService _gameSoundsService;

        private readonly Dictionary<Entity, MonoEntity> _entityToMono = new();

        public MonoEntitiesFactory(
            ResourcesAssetsLoader resources,
            EntitiesLifeContext entitiesLifeContext,
            CollidersRegistryService collidersRegistryService,
            IGameSoundsService gameSoundsService)
        {
            _resources = resources;
            _entitiesLifeContext = entitiesLifeContext;
            _collidersRegistryService = collidersRegistryService;
            _gameSoundsService = gameSoundsService;
        }

        public MonoEntity Create(Entity entity, Vector3 position, string path)
        {
            MonoEntity prefab = _resources.Load<MonoEntity>(path);

            MonoEntity viewInstance = Object.Instantiate(prefab, position, Quaternion.identity, null);

            viewInstance.Initialize(_collidersRegistryService, _gameSoundsService);

            viewInstance.Link(entity);

            _entityToMono.Add(entity, viewInstance);

            return viewInstance;
        }

        public void Initialize()
        {
            _entitiesLifeContext.Released += OnEntityReleased;
        }

        public void Dispose()
        {
            _entitiesLifeContext.Released -= OnEntityReleased;

            foreach (Entity entity in _entityToMono.Keys)
                CleanupFor(entity);

            _entityToMono.Clear();
        }

        private void OnEntityReleased(Entity entity)
        {
            CleanupFor(entity);

            _entityToMono.Remove(entity);
        }

        private void CleanupFor(Entity entity)
        {
            MonoEntity monoEntity = _entityToMono[entity];
            monoEntity.Cleanup(entity);
            Object.Destroy(monoEntity.gameObject);
        }
    }
}
