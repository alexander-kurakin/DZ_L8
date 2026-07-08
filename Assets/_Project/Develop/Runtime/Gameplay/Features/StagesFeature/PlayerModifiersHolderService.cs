using System;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.ProjectileModifiers;
using Assets._Project.Develop.Runtime.Utilities.Reactive;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature
{
    public class PlayerModifiersHolderService
    {
        private readonly ProjectileModifiersFactory _projectileModifiersFactory;
        private readonly ReactiveEvent<Entity> _playerRegistered = new();

        private Entity _playerEntity;

        public PlayerModifiersHolderService(ProjectileModifiersFactory projectileModifiersFactory)
        {
            _projectileModifiersFactory = projectileModifiersFactory;
        }

        public Entity PlayerEntity => _playerEntity;

        public IReadOnlyEvent<Entity> HeroRegistred => _playerRegistered;

        public void Create()
        {
            if (_playerEntity != null)
                return;

            _playerEntity = new Entity();
            _projectileModifiersFactory.EquipAllModifiers(_playerEntity);
            _playerRegistered.Invoke(_playerEntity);
        }
    }
}
