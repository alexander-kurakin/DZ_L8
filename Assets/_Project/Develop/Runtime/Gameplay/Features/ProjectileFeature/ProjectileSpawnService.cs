using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.ProjectileModifiers;
using Assets._Project.Develop.Runtime.Infrastructure.DI;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.ProjectileFeature
{
    public class ProjectileSpawnService
    {
        private readonly EntitiesFactory _entitiesFactory;
        private readonly ProjectileModifiersFactory _projectileModifiersFactory;

        public ProjectileSpawnService(DIContainer container)
        {
            _entitiesFactory = container.Resolve<EntitiesFactory>();
            _projectileModifiersFactory = container.Resolve<ProjectileModifiersFactory>();
        }

        public Entity Spawn(Vector3 position, Vector3 direction, float damage, Entity owner, ModifierType modifierType)
        {
            Entity projectile = _entitiesFactory.CreateEmpty();
            IProjectileModifierBehavior behavior = _projectileModifiersFactory.CreateBehavior(modifierType);
            behavior.ApplyToProjectile(projectile, damage, 0f);
            return projectile;
        }
    }
}
