using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Throw;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using Assets._Project.Develop.Runtime.Gameplay.Features.ContactTakeDamage;
using Assets._Project.Develop.Runtime.Gameplay.Features.LifeCycle;
using Assets._Project.Develop.Runtime.Gameplay.Features.MainHero;
using Assets._Project.Develop.Runtime.Gameplay.Features.MovementFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.ProjectileFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.Sensors;
using Assets._Project.Develop.Runtime.Gameplay.Features.TeamsFeature;
using Assets._Project.Develop.Runtime.Infrastructure.DI;
using Assets._Project.Develop.Runtime.Utilities;
using Assets._Project.Develop.Runtime.Utilities.Conditions;
using Assets._Project.Develop.Runtime.Utilities.ConfigsManagment;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System.Collections.Generic;
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
        private readonly ThrowChargeConfig _throwChargeConfig;
        private readonly List<Entity> _projectilesInScene = new();

        public EntitiesFactory(DIContainer container)
        {
            _container = container;
            _entitiesLifeContext = _container.Resolve<EntitiesLifeContext>();
            _monoEntitiesFactory = _container.Resolve<MonoEntitiesFactory>();
            _collidersRegistryService = _container.Resolve<CollidersRegistryService>();
            _mainHeroHolderService = _container.Resolve<MainHeroHolderService>();
            _throwChargeConfig = _container.Resolve<ConfigsProviderService>().GetConfig<ThrowChargeConfig>();
            _entitiesLifeContext.Released += OnEntityReleased;
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
            ReleaseExcessProjectiles();

            Entity entity = CreateEmpty();

            _monoEntitiesFactory.Create(entity, position, "Entities/Projectile");

            Rigidbody rigidbody = entity.Rigidbody;
            Teams projectileTeam = ResolveProjectileTeam(owner);

            entity
                .AddMoveDirection(new ReactiveVariable<Vector3>(Vector3.zero))
                .AddMoveSpeed(new ReactiveVariable<float>(0f))
                .AddIsMoving()
                .AddRotationDirection(new ReactiveVariable<Vector3>(Vector3.zero))
                .AddRotationSpeed(new ReactiveVariable<float>(9999f))
                .AddProjectileSpeed(new ReactiveVariable<float>(0f))
                .AddProjectileDamage(new ReactiveVariable<float>(0f))
                .AddProjectileOwner(owner)
                .AddHasCollided(new ReactiveVariable<bool>(false))
                .AddProjectileImpacted(new ReactiveEvent<Vector3>())
                .AddIsDead()
                .AddContactsDetectingMask(Layers.ProjectileContactsMask)
                .AddContactCollidersBuffer(new Buffer<Collider>(64))
                .AddContactEntitiesBuffer(new Buffer<Entity>(64))
                .AddBodyContactDamage(new ReactiveVariable<float>(1f))
                .AddComponent(new ContactDamageOwner { Value = owner })
                .AddIsTouchAnotherTeam()
                .AddTeam(new ReactiveVariable<Teams>(projectileTeam));

            ICompositeCondition canMove = new CompositeCondition()
                .Add(new FuncCondition(() => entity.IsDead.Value == false))
                .Add(new FuncCondition(() => rigidbody.isKinematic == false));

            ICompositeCondition canRotate = new CompositeCondition()
                .Add(new FuncCondition(() => entity.IsDead.Value == false));

            ICompositeCondition mustDie = new CompositeCondition()
                .Add(new FuncCondition(() => entity.IsTouchAnotherTeam.Value));

            ICompositeCondition mustSelfRelease = new CompositeCondition()
                .Add(new FuncCondition(() => entity.IsDead.Value == true));

            entity
                .AddCanMove(canMove)
                .AddCanRotate(canRotate)
                .AddMustDie(mustDie)
                .AddMustSelfRelease(mustSelfRelease);

            entity
                .AddSystem(new RigidbodyMovementSystem())
                .AddSystem(new RigidbodyRotationSystem())
                .AddSystem(new BodyContactsDetectingSystem(ColliderType.Sphere))
                .AddSystem(new BodyContactsEntitiesFilterSystem(_collidersRegistryService))
                .AddSystem(new DealDamageOnContactSystem())
                .AddSystem(new AnotherTeamTouchDetectorSystem())
                .AddSystem(new ProjectileMaxDistanceFromOwnerSystem(_throwChargeConfig.ProjectileMaxDistanceFromOwner))
                .AddSystem(new DeathSystem())
                .AddSystem(new DisableCollidersOnDeathSystem())
                .AddSystem(new SelfReleaseSystem(_entitiesLifeContext));

            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;

            _entitiesLifeContext.Add(entity);
            _projectilesInScene.Add(entity);

            return entity;
        }

        public Entity CreateEmpty() => new Entity();

        private Teams ResolveProjectileTeam(Entity owner)
        {
            if (owner != null && owner.TryGetTeam(out ReactiveVariable<Teams> ownerTeam))
                return ownerTeam.Value;

            return Teams.MainHero;
        }

        private void OnEntityReleased(Entity entity)
        {
            _projectilesInScene.Remove(entity);
        }

        private void ReleaseExcessProjectiles()
        {
            int maxProjectilesInScene = _throwChargeConfig.MaxProjectilesInScene;

            while (_projectilesInScene.Count >= maxProjectilesInScene)
            {
                Entity oldestProjectile = _projectilesInScene[0];
                _projectilesInScene.RemoveAt(0);
                _entitiesLifeContext.Release(oldestProjectile);
            }
        }
    }
}
