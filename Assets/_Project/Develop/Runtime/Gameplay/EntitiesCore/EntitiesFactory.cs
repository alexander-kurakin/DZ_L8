using _Project.Develop.Runtime.Gameplay.Features.DealAreaDamage;
using _Project.Develop.Runtime.Gameplay.Features.DealDamageOverTime;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Levels;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using Assets._Project.Develop.Runtime.Gameplay.Features.AI;
using Assets._Project.Develop.Runtime.Gameplay.Features.AI.States;
using Assets._Project.Develop.Runtime.Gameplay.Features.Attack;
using Assets._Project.Develop.Runtime.Gameplay.Features.Attack.Shoot;
using Assets._Project.Develop.Runtime.Gameplay.Features.ContactTakeDamage;
using Assets._Project.Develop.Runtime.Gameplay.Features.DealDamageOnTargetReached;
using Assets._Project.Develop.Runtime.Gameplay.Features.DistanceDetector;
using Assets._Project.Develop.Runtime.Gameplay.Features.GameplayStateBridge;
using Assets._Project.Develop.Runtime.Gameplay.Features.LifeCycle;
using Assets._Project.Develop.Runtime.Gameplay.Features.MainHero;
using Assets._Project.Develop.Runtime.Gameplay.Features.Mines;
using Assets._Project.Develop.Runtime.Gameplay.Features.MovementFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.Sensors;
using Assets._Project.Develop.Runtime.Gameplay.Features.SpawnFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.TakeDamage;
using Assets._Project.Develop.Runtime.Gameplay.Features.TeamsFeature;
using Assets._Project.Develop.Runtime.Infrastructure.DI;
using Assets._Project.Develop.Runtime.Utilities;
using Assets._Project.Develop.Runtime.Utilities.Conditions;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
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
        private readonly BrainsFactory _brainsFactory;

        public EntitiesFactory(DIContainer container)
        {
            _container = container;
            _entitiesLifeContext = _container.Resolve<EntitiesLifeContext>();
            _monoEntitiesFactory = _container.Resolve<MonoEntitiesFactory>();
            _collidersRegistryService = _container.Resolve<CollidersRegistryService>();
            _mainHeroHolderService = _container.Resolve<MainHeroHolderService>();
            _brainsFactory = _container.Resolve<BrainsFactory>();
        }

        public Entity CreateTower(TowerConfig config, LevelConfig levelConfig)
        {
            Entity entity = CreateEmpty();
            Vector3 startPosition = config.StartPosition;
            
            _monoEntitiesFactory.Create(entity, startPosition, config.PrefabPath);

            entity
                .AddMaxHealth(new ReactiveVariable<float>(levelConfig.TowerMaxHealth))
                .AddCurrentHealth(new ReactiveVariable<float>(levelConfig.TowerMaxHealth))
                .AddIsDead()
                .AddInDeathProcess()
                .AddDeathProcessInitialTime(new ReactiveVariable<float>(config.DeathProcessTime))
                .AddDeathProcessCurrentTime()
                .AddTakeDamageRequest()
                .AddTakeDamageEvent();

            ICompositeCondition mustDie = new CompositeCondition()
                .Add(new FuncCondition(() => entity.CurrentHealth.Value <= 0));
            
            ICompositeCondition mustSelfRelease = new CompositeCondition()
                .Add(new FuncCondition(() => entity.IsDead.Value))
                .Add(new FuncCondition(() => entity.InDeathProcess.Value == false));
            
            ICompositeCondition canTakeIncomingDamage = new CompositeCondition()
                .Add(new FuncCondition(() => entity.IsDead.Value == false));

            entity
                .AddMustDie(mustDie)
                .AddMustSelfRelease(mustSelfRelease)
                .AddCanTakeDamage(canTakeIncomingDamage);

            entity
                .AddSystem(new TakeDamageSystem())
                .AddSystem(new DeathSystem())
                .AddSystem(new DisableCollidersOnDeathSystem())
                .AddSystem(new DeathProcessTimerSystem())
                .AddSystem(new SelfReleaseSystem(_entitiesLifeContext));
            
            return entity;
        }

        public Entity CreateTowerWalker(Vector3 position)
        {
            Entity entity = CreateEmpty();

            _monoEntitiesFactory.Create(entity, position, "Entities/TowerWalker");

            entity
                .AddMoveDirection()
                .AddMoveSpeed(new ReactiveVariable<float>(3))
                .AddIsMoving()
                .AddRotationDirection()
                .AddRotationSpeed(new ReactiveVariable<float>(900))
                .AddMagicCastRequestedEvent();
            
            ICompositeCondition canMove = new CompositeCondition()
                .Add(new FuncCondition(() => entity.GameplayPhase.Value == GameplayStates.StageProcess));
            
            ICompositeCondition canRotate = new CompositeCondition()
                .Add(new FuncCondition(() => true)); //tower walker always rotates towards mouse cursor
            
            ICompositeCondition mustSelfRelease = new CompositeCondition()
                .Add(new FuncCondition(() => _mainHeroHolderService.MainHero.IsDead.Value))
                .Add(new FuncCondition(() => _mainHeroHolderService.MainHero.InDeathProcess.Value == false));

            entity
                .AddCanMove(canMove)
                .AddCanRotate(canRotate)
                .AddMustSelfRelease(mustSelfRelease);

            entity
                .AddSystem(new RigidbodyMovementSystem())
                .AddSystem(new RigidbodyRotationSystem())
                .AddSystem(new SelfReleaseSystem(_entitiesLifeContext));

            return entity;
        }
        
        public Entity CreateRangedEnemy(Vector3 position, RangedEnemyConfig config)
        {
            Entity entity = CreateEmpty();

            _monoEntitiesFactory.Create(entity, position, config.PrefabPath);

            entity
                .AddMoveDirection()
                .AddMoveSpeed(new ReactiveVariable<float>(config.MoveSpeed))
                .AddIsMoving()
                .AddRotationDirection()
                .AddRotationSpeed(new ReactiveVariable<float>(config.RotationSpeed))
                .AddMaxHealth(new ReactiveVariable<float>(config.MaxHealth))
                .AddCurrentHealth(new ReactiveVariable<float>(config.MaxHealth))
                .AddIsDead()
                .AddInDeathProcess()
                .AddDeathProcessInitialTime(new ReactiveVariable<float>(config.DeathProcessTime))
                .AddDeathProcessCurrentTime()
                .AddTakeDamageRequest()
                .AddTakeDamageEvent()
                .AddCurrentTarget()
                .AddDistanceToTargetGoal(new ReactiveVariable<float>(config.AttackRange))
                .AddDistanceToTargetCurrent(new ReactiveVariable<float>(config.AttackRange))
                .AddDistanceToTargetReachedEvent()
                .AddDistanceToTargetReached()
                .AddAttackProcessInitialTime(new ReactiveVariable<float>(config.AttackProcessTime))
                .AddAttackProcessCurrentTime()
                .AddInAttackProcess()
                .AddStartAttackRequest()
                .AddStartAttackEvent()
                .AddEndAttackEvent()
                .AddAttackDelayTime(new ReactiveVariable<float>(config.AttackDelayTime))
                .AddAttackDelayEndEvent()
                .AddAttackCooldownInitialTime(new ReactiveVariable<float>(config.AttackCooldown))
                .AddAttackCooldownCurrentTime()
                .AddInAttackCooldown()
                .AddInstantAttackDamage(new ReactiveVariable<float>(config.InstantDamage))
                .AddSpawnInitialTime(new ReactiveVariable<float>(config.SpawnProcessTime))
                .AddSpawnCurrentTime()
                .AddInSpawnProcess();
            
            ICompositeCondition canMove = new CompositeCondition()
                .Add(new FuncCondition(() => entity.IsDead.Value == false))
                .Add(new FuncCondition(() => entity.InSpawnProcess.Value == false))
                .Add(new FuncCondition(() => entity.DistanceToTargetReached.Value == false));

            ICompositeCondition canRotate = new CompositeCondition()
                .Add(new FuncCondition(() => entity.IsDead.Value == false))
                .Add(new FuncCondition(() => entity.InSpawnProcess.Value == false));

            ICompositeCondition mustDie = new CompositeCondition(LogicOperations.Or)
                .Add(new FuncCondition(() => entity.CurrentHealth.Value <= 0));

            ICompositeCondition mustSelfRelease = new CompositeCondition()
                .Add(new FuncCondition(() => entity.IsDead.Value))
                .Add(new FuncCondition(() => entity.InDeathProcess.Value == false));

            ICompositeCondition canTakeDamage = new CompositeCondition()
                .Add(new FuncCondition(() => entity.InSpawnProcess.Value == false))
                .Add(new FuncCondition(() => entity.IsDead.Value == false));
            
            ICompositeCondition canStartAttack = new CompositeCondition()
                .Add(new FuncCondition(() => entity.InAttackProcess.Value == false))
                .Add(new FuncCondition(() => entity.InAttackCooldown.Value == false))
                .Add(new FuncCondition(() => entity.DistanceToTargetReached.Value));

            entity
                .AddCanMove(canMove)
                .AddCanRotate(canRotate)
                .AddMustDie(mustDie)
                .AddMustSelfRelease(mustSelfRelease)
                .AddCanTakeDamage(canTakeDamage)
                .AddCanStartAttack(canStartAttack);

            entity
                .AddSystem(new SpawnProcessTimerSystem())
                .AddSystem(new RigidbodyMovementSystem())
                .AddSystem(new RigidbodyRotationSystem())
                .AddSystem(new TakeDamageSystem())
                .AddSystem(new DeathSystem())
                .AddSystem(new DisableCollidersOnDeathSystem())
                .AddSystem(new DeathProcessTimerSystem())
                .AddSystem(new SelfReleaseSystem(_entitiesLifeContext))
                .AddSystem(new DistanceDetectorSystem())
                .AddSystem(new StartAttackSystem())
                .AddSystem(new AttackProcessTimerSystem())
                .AddSystem(new AttackDelayEndTriggerSystem())
                .AddSystem(new InstantShootSystem(this))
                .AddSystem(new EndAttackSystem())
                .AddSystem(new AttackCooldownTimerSystem());

            return entity;
        }

        public Entity CreateWalkingEnemy(Vector3 position, WalkingEnemyConfig config)
        {
            Entity entity = CreateEmpty();

            _monoEntitiesFactory.Create(entity, position, config.PrefabPath);

            entity
                .AddMoveDirection()
                .AddMoveSpeed(new ReactiveVariable<float>(config.MoveSpeed))
                .AddIsMoving()
                .AddRotationDirection()
                .AddRotationSpeed(new ReactiveVariable<float>(config.RotationSpeed))
                .AddMaxHealth(new ReactiveVariable<float>(config.MaxHealth))
                .AddCurrentHealth(new ReactiveVariable<float>(config.MaxHealth))
                .AddIsDead()
                .AddInDeathProcess()
                .AddDeathProcessInitialTime(new ReactiveVariable<float>(config.DeathProcessTime))
                .AddDeathProcessCurrentTime()
                .AddTakeDamageRequest()
                .AddTakeDamageEvent()
                .AddCurrentTarget()
                .AddDistanceToTargetGoal(new ReactiveVariable<float>(config.DistanceToTargetGoal))
                .AddDistanceToTargetCurrent(new ReactiveVariable<float>(config.DistanceToTargetGoal))
                .AddDistanceToTargetReachedEvent()
                .AddDistanceToTargetReached()
                .AddExplosionDamage(new ReactiveVariable<float>(config.ExplosionDamage))
                .AddSpawnInitialTime(new ReactiveVariable<float>(config.SpawnProcessTime))
                .AddSpawnCurrentTime()
                .AddInSpawnProcess();
            
            ICompositeCondition canMove = new CompositeCondition()
                .Add(new FuncCondition(() => entity.IsDead.Value == false))
                .Add(new FuncCondition(() => entity.InSpawnProcess.Value == false))
                .Add(new FuncCondition(() => entity.DistanceToTargetReached.Value == false));

            ICompositeCondition canRotate = new CompositeCondition()
                .Add(new FuncCondition(() => entity.IsDead.Value == false))
                .Add(new FuncCondition(() => entity.InSpawnProcess.Value == false))
                .Add(new FuncCondition(() => entity.DistanceToTargetReached.Value == false));

            ICompositeCondition mustDie = new CompositeCondition(LogicOperations.Or)
                .Add(new FuncCondition(() => entity.CurrentHealth.Value <= 0))
                .Add(new FuncCondition(() => entity.DistanceToTargetReached.Value));

            ICompositeCondition mustSelfRelease = new CompositeCondition()
                .Add(new FuncCondition(() => entity.IsDead.Value))
                .Add(new FuncCondition(() => entity.InDeathProcess.Value == false));

            ICompositeCondition canTakeDamage = new CompositeCondition()
                .Add(new FuncCondition(() => entity.InSpawnProcess.Value == false))
                .Add(new FuncCondition(() => entity.IsDead.Value == false));
            
            entity
                .AddCanMove(canMove)
                .AddCanRotate(canRotate)
                .AddMustDie(mustDie)
                .AddMustSelfRelease(mustSelfRelease)
                .AddCanTakeDamage(canTakeDamage);
            
            entity
                .AddSystem(new SpawnProcessTimerSystem())
                .AddSystem(new RigidbodyMovementSystem())
                .AddSystem(new RigidbodyRotationSystem())
                .AddSystem(new TakeDamageSystem())
                .AddSystem(new DeathSystem())
                .AddSystem(new DisableCollidersOnDeathSystem())
                .AddSystem(new DeathProcessTimerSystem())
                .AddSystem(new SelfReleaseSystem(_entitiesLifeContext))
                .AddSystem(new DistanceDetectorSystem())
                .AddSystem(new DealDamageOnTargetReachedSystem());

            return entity;
        }

        public Entity CreateContactTrigger(Vector3 position, ContactTriggerConfig config)
        {
            Entity entity = CreateEmpty();

            _monoEntitiesFactory.Create(entity, position, config.PrefabPath);

            entity
                .AddContactsDetectingMask(Layers.CharactersMask)
                .AddContactCollidersBuffer(new Buffer<Collider>(64))
                .AddContactEntitiesBuffer(new Buffer<Entity>(64));

            entity
                .AddSystem(new BodyContactsDetectingSystem(ColliderType.Capsule))
                .AddSystem(new BodyContactsEntitiesFilterSystem(_collidersRegistryService));

            _entitiesLifeContext.Add(entity);

            return entity;
        }

        public Entity CreateMine(Vector3 position, MineConfig mineConfig)
        {
            Entity entity = CreateEmpty();
            
            _monoEntitiesFactory.Create(entity, position, mineConfig.PrefabPath);
            
            entity
                .AddContactsDetectingMask(Layers.CharactersMask)
                .AddContactCollidersBuffer(new Buffer<Collider>(64))
                .AddContactEntitiesBuffer(new Buffer<Entity>(64))
                .AddTeam(new ReactiveVariable<Teams>(Teams.MainHero))
                .AddAreaImpactDamage(new ReactiveVariable<float>(mineConfig.Damage))
                .AddAreaImpactRadius(new ReactiveVariable<float>(mineConfig.ExplosionRadius))
                .AddAreaImpactMask(Layers.CharactersMask)
                .AddAreaImpactCollidersBuffer(new Buffer<Collider>(64))
                .AddAreaImpactEntitiesBuffer(new Buffer<Entity>(64))
                .AddDealAreaImpactDamageRequest();
                
            entity
                .AddSystem(new BodyContactsDetectingSystem(ColliderType.Sphere))
                .AddSystem(new BodyContactsEntitiesFilterSystem(_collidersRegistryService))
                .AddSystem(new MineDetonationSystem(_entitiesLifeContext))
                .AddSystem(new AreaDamageContactDetectingSystem(_collidersRegistryService))
                .AddSystem(new AreaDamageEntitiesFilterSystem(_collidersRegistryService))
                .AddSystem(new DealAreaDamageSystem());

            _entitiesLifeContext.Add(entity);
            
            return entity;
        }
        
        public Entity CreateTurret(Vector3 position, TurretConfig turretConfig)
        {
            Entity entity = CreateEmpty();
            
            _monoEntitiesFactory.Create(entity, position, turretConfig.PrefabPath);

            entity
                .AddTeam(new ReactiveVariable<Teams>(Teams.MainHero))
                .AddRotationDirection()
                .AddRotationSpeed(new ReactiveVariable<float>(turretConfig.RotationSpeed))
                .AddCurrentTarget()
                .AddAttackProcessInitialTime(new ReactiveVariable<float>(turretConfig.AttackProcessTime))
                .AddAttackProcessCurrentTime()
                .AddInAttackProcess()
                .AddStartAttackRequest()
                .AddStartAttackEvent()
                .AddEndAttackEvent()
                .AddAttackDelayTime(new ReactiveVariable<float>(turretConfig.AttackDelayTime))
                .AddAttackDelayEndEvent()
                .AddAttackCooldownInitialTime(new ReactiveVariable<float>(turretConfig.AttackCooldown))
                .AddAttackCooldownCurrentTime()
                .AddInAttackCooldown()
                .AddInstantAttackDamage(new ReactiveVariable<float>(turretConfig.Damage));
            
            ICompositeCondition canRotate = new CompositeCondition()
                .Add(new FuncCondition(() => entity.CurrentTarget.Value != null));
            
            ICompositeCondition canStartAttack = new CompositeCondition()
                .Add(new FuncCondition(() => entity.InAttackProcess.Value == false))
                .Add(new FuncCondition(() => entity.InAttackCooldown.Value == false));

            entity
                .AddCanRotate(canRotate)
                .AddCanStartAttack(canStartAttack);

            entity
                .AddSystem(new RigidbodyRotationSystem())
                .AddSystem(new StartAttackSystem())
                .AddSystem(new AttackProcessTimerSystem())
                .AddSystem(new AttackDelayEndTriggerSystem())
                .AddSystem(new InstantShootSystem(this))
                .AddSystem(new EndAttackSystem())
                .AddSystem(new AttackCooldownTimerSystem());
                
            _entitiesLifeContext.Add(entity);
            
            _brainsFactory.CreateTurretBrain(entity, new NearestDamageableTargetSelector(entity));
            
            return entity;
        }
        
        public Entity CreateToxicArea(Vector3 position, ToxicAreaConfig toxicAreaConfig)
        {
            Entity entity = CreateEmpty();
            
            _monoEntitiesFactory.Create(entity, position, toxicAreaConfig.PrefabPath);

            entity
                .AddContactsDetectingMask(Layers.CharactersMask)
                .AddContactCollidersBuffer(new Buffer<Collider>(64))
                .AddContactEntitiesBuffer(new Buffer<Entity>(64))
                .AddDamagePerTick(new ReactiveVariable<float>(toxicAreaConfig.DamagePerTick))
                .AddDamageInterval(new ReactiveVariable<float>(toxicAreaConfig.DamageInterval))
                .AddDamageTimer(new ReactiveVariable<float>(toxicAreaConfig.DamageInterval))
                .AddTeam(new ReactiveVariable<Teams>(Teams.MainHero));
            
            entity
                .AddSystem(new BodyContactsDetectingSystem(ColliderType.Sphere))
                .AddSystem(new BodyContactsEntitiesFilterSystem(_collidersRegistryService))
                .AddSystem(new DamageOverTimeSystem());

            _entitiesLifeContext.Add(entity);
            
            return entity;
        }
        
        public Entity CreateProjectile(Vector3 position, Vector3 direction, float damage, Entity owner)
        {
            Entity entity = CreateEmpty();

            _monoEntitiesFactory.Create(entity, position, "Entities/Projectile");

            entity
                .AddMoveDirection(new ReactiveVariable<Vector3>(direction))
                .AddMoveSpeed(new ReactiveVariable<float>(25))
                .AddIsMoving()
                .AddRotationDirection(new ReactiveVariable<Vector3>(direction))
                .AddRotationSpeed(new ReactiveVariable<float>(9999))
                .AddIsDead()
                .AddContactsDetectingMask(Layers.CharactersMask | Layers.EnviromentMask)
                .AddContactCollidersBuffer(new Buffer<Collider>(64))
                .AddContactEntitiesBuffer(new Buffer<Entity>(64))
                .AddBodyContactDamage(new ReactiveVariable<float>(damage))
                .AddDeathMask(Layers.EnviromentMask)
                .AddIsTouchDeathMask()
                .AddIsTouchAnotherTeam()
                .AddTeam(new ReactiveVariable<Teams>(owner.Team.Value));

            ICompositeCondition canMove = new CompositeCondition()
                .Add(new FuncCondition(() => entity.IsDead.Value == false));

            ICompositeCondition canRotate = new CompositeCondition()
                .Add(new FuncCondition(() => entity.IsDead.Value == false));

            ICompositeCondition mustDie = new CompositeCondition(LogicOperations.Or)
                .Add(new FuncCondition(() => entity.IsTouchDeathMask.Value))
                .Add(new FuncCondition(() => entity.IsTouchAnotherTeam.Value));

            ICompositeCondition mustSelfRelease = new CompositeCondition()
                .Add(new FuncCondition(() => entity.IsDead.Value));

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
                .AddSystem(new DeathMaskTouchDetectorSystem())
                .AddSystem(new AnotherTeamTouchDetectorSystem())
                .AddSystem(new DeathSystem())
                .AddSystem(new DisableCollidersOnDeathSystem())
                .AddSystem(new SelfReleaseSystem(_entitiesLifeContext));

            _entitiesLifeContext.Add(entity);

            return entity;
        }

        private Entity CreateEmpty() => new Entity();
    }
}
