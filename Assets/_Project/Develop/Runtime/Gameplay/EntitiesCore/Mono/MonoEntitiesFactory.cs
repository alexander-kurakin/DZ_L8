using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using Assets._Project.Develop.Runtime.Infrastructure.DI;
using Assets._Project.Develop.Runtime.Utilities.AssetsManagment;
using Assets._Project.Develop.Runtime.Utilities.Audio;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets._Project.Develop.Runtime.Gameplay.EntitiesCore
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
            return Create(entity, position, Quaternion.identity, path);
        }

        public MonoEntity Create(Entity entity, Vector3 position, Quaternion rotation, string path)
        {
            if (_entityToMono.TryGetValue(entity, out MonoEntity existingMonoEntity))
                return existingMonoEntity;

            MonoEntity prefab = _resources.Load<MonoEntity>(path);

            MonoEntity viewInstance = Object.Instantiate(prefab, position, rotation, null);

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
            if (_entityToMono.TryGetValue(entity, out MonoEntity monoEntity) == false)
                return;

            monoEntity.Cleanup(entity);
            Object.Destroy(monoEntity.gameObject);
        }
    }
}
