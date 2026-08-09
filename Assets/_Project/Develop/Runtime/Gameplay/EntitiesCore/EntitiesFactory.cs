using _Project.Develop.Runtime.Gameplay.Features.DealAreaDamage;
using _Project.Develop.Runtime.Gameplay.Features.Input;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Levels;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Spellcore;
using Assets._Project.Develop.Runtime.Gameplay.Infrastructure;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Sectors;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using Assets._Project.Develop.Runtime.Gameplay.Features.AI;
using Assets._Project.Develop.Runtime.Gameplay.Features.Attack;
using Assets._Project.Develop.Runtime.Gameplay.Features.Attack.Shoot;
using Assets._Project.Develop.Runtime.Gameplay.Features.ContactTakeDamage;
using Assets._Project.Develop.Runtime.Gameplay.Features.DealDamageOnTargetReached;
using Assets._Project.Develop.Runtime.Gameplay.Features.DistanceDetector;
using Assets._Project.Develop.Runtime.Gameplay.Features.GameplayStateBridge;
using Assets._Project.Develop.Runtime.Gameplay.Features.JuiceFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.LifeCycle;
using Assets._Project.Develop.Runtime.Gameplay.Features.MainHero;
using Assets._Project.Develop.Runtime.Gameplay.Features.Mines;
using Assets._Project.Develop.Runtime.Gameplay.Features.MovementFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.PlantBuildingBuff;
using Assets._Project.Develop.Runtime.Gameplay.Features.PlantCombatFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.Sensors;
using Assets._Project.Develop.Runtime.Gameplay.Features.SpawnFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.TakeDamage;
using Assets._Project.Develop.Runtime.Gameplay.Features.TowerIntegrityFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.TowerWalker;
using Assets._Project.Develop.Runtime.Gameplay.Features.TeamsFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.EssenceFeature;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Essence;
using Assets._Project.Develop.Runtime.Infrastructure.DI;
using Assets._Project.Develop.Runtime.Utilities;
using Assets._Project.Develop.Runtime.Utilities.Conditions;
using Assets._Project.Develop.Runtime.Utilities.ConfigsManagment;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;
using Object = UnityEngine.Object;

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

            TowerIntegrityConfig towerIntegrityConfig =
                _container.Resolve<ConfigsProviderService>().GetConfig<TowerIntegrityConfig>();
            float towerMaxHealth = towerIntegrityConfig.MaxHits;

            entity
                .AddMaxHealth(new ReactiveVariable<float>(towerMaxHealth))
                .AddCurrentHealth(new ReactiveVariable<float>(towerMaxHealth))
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

            entity.AddSystem(new TowerIntegrityTakeDamageSystem(
                new TowerIntegrityLeakResolver(towerIntegrityConfig)));

            entity
                .AddSystem(new TowerTakeDamageScreenShakeSystem(_container.Resolve<GameplayJuiceService>()))
                .AddSystem(new DeathSystem())
                .AddSystem(new DisableCollidersOnDeathSystem())
                .AddSystem(new DeathProcessTimerSystem())
                .AddSystem(new SelfReleaseSystem(_entitiesLifeContext));
            
            return entity;
        }
        
        public Entity CreateTowerBrother(Vector3 position)
        {
            Entity entity = CreateEmpty();

            _monoEntitiesFactory.Create(entity, position, "Entities/TowerBrother");

            entity
                .AddMoveDirection()
                .AddMoveSpeed(new ReactiveVariable<float>(3))
                .AddIsMoving()
                .AddIsCurrentlyIdle()
                .AddRotationDirection()
                .AddRotationSpeed(new ReactiveVariable<float>(900));
            
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
        
        public Entity CreateRangedShootingEnemy(Vector3 position, RangedShootingEnemyConfig config)
        {
            Entity entity = CreateEmpty();

            _monoEntitiesFactory.Create(entity, position, config.PrefabPath);

            SectorGridConfig sectorGridConfig = _container.Resolve<ConfigsProviderService>().GetConfig<SectorGridConfig>();
            float stopDistance = config.GetStopDistance(sectorGridConfig);

            entity
                .AddMoveDirection()
                .AddMoveSpeed(new ReactiveVariable<float>(ResolveEnemyMoveSpeed(config.MoveSpeed)))
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
                .AddDistanceToTargetGoal(new ReactiveVariable<float>(stopDistance))
                .AddDistanceToTargetCurrent(new ReactiveVariable<float>(stopDistance))
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
                .AddInSpawnProcess()
                .AddEnemySpawnOrigin(position)
                .AddEnemyHitStunRemainingTime(new ReactiveVariable<float>(0f));
            
            ICompositeCondition canMove = new CompositeCondition()
                .Add(new FuncCondition(() => entity.IsDead.Value == false))
                .Add(new FuncCondition(() => entity.InSpawnProcess.Value == false))
                .Add(new FuncCondition(() => entity.DistanceToTargetReached.Value == false))
                .Add(new FuncCondition(() => entity.EnemyHitStunRemainingTime.Value <= 0f));

            ICompositeCondition canRotate = new CompositeCondition()
                .Add(new FuncCondition(() => entity.IsDead.Value == false))
                .Add(new FuncCondition(() => entity.InSpawnProcess.Value == false))
                .Add(new FuncCondition(() => entity.EnemyHitStunRemainingTime.Value <= 0f));

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
                .AddSystem(new EnemyHitReactionSystem())
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
        
        public Entity CreateRangedDoTWalkingEnemy(Vector3 position, RangedDotWalkingEnemyConfig config)
        {
            Entity entity = CreateEmpty();

            _monoEntitiesFactory.Create(entity, position, config.PrefabPath);

            entity
                .AddMoveDirection()
                .AddMoveSpeed(new ReactiveVariable<float>(ResolveEnemyMoveSpeed(config.MoveSpeed)))
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
                .AddDamageInterval(new ReactiveVariable<float>(config.DamageInterval))
                .AddDamagePerTick(new ReactiveVariable<float>(config.DamagePerTick))
                .AddDamageTimer()
                .AddDragonEnrageStackCount(0)
                .AddComponent(new FlyingEnemy())
                .AddSpawnInitialTime(new ReactiveVariable<float>(config.SpawnProcessTime))
                .AddSpawnCurrentTime()
                .AddInSpawnProcess()
                .AddEnemySpawnOrigin(position)
                .AddEnemyHitStunRemainingTime(new ReactiveVariable<float>(0f));
            
            ICompositeCondition canMove = new CompositeCondition()
                .Add(new FuncCondition(() => entity.IsDead.Value == false))
                .Add(new FuncCondition(() => entity.InSpawnProcess.Value == false))
                .Add(new FuncCondition(() => entity.DistanceToTargetReached.Value == false))
                .Add(new FuncCondition(() => entity.EnemyHitStunRemainingTime.Value <= 0f));

            ICompositeCondition canRotate = new CompositeCondition()
                .Add(new FuncCondition(() => entity.IsDead.Value == false))
                .Add(new FuncCondition(() => entity.InSpawnProcess.Value == false))
                .Add(new FuncCondition(() => entity.DistanceToTargetReached.Value == false))
                .Add(new FuncCondition(() => entity.EnemyHitStunRemainingTime.Value <= 0f));

            ICompositeCondition mustDie = new CompositeCondition(LogicOperations.Or)
                .Add(new FuncCondition(() => entity.CurrentHealth.Value <= 0));

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
                .AddSystem(new EnemyHitReactionSystem())
                .AddSystem(new TakeDamageSystem())
                .AddSystem(new DeathSystem())
                .AddSystem(new DisableCollidersOnDeathSystem())
                .AddSystem(new DeathProcessTimerSystem())
                .AddSystem(new SelfReleaseSystem(_entitiesLifeContext))
                .AddSystem(new DistanceDetectorSystem())
                .AddSystem(new DealDoTDamageOnTargetReachedSystem(_container.Resolve<DragonEnrageService>()));

            return entity;
        }

        public Entity CreateExplodingWalkingEnemy(Vector3 position, ExplodingWalkingEnemyConfig config)
        {
            Entity entity = CreateEmpty();

            _monoEntitiesFactory.Create(entity, position, config.PrefabPath);

            SectorGridConfig sectorGridConfig = _container.Resolve<ConfigsProviderService>().GetConfig<SectorGridConfig>();
            float stopDistance = config.GetStopDistance(sectorGridConfig);

            entity
                .AddMoveDirection()
                .AddMoveSpeed(new ReactiveVariable<float>(ResolveEnemyMoveSpeed(config.MoveSpeed)))
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
                .AddDistanceToTargetGoal(new ReactiveVariable<float>(stopDistance))
                .AddDistanceToTargetCurrent(new ReactiveVariable<float>(stopDistance))
                .AddDistanceToTargetReachedEvent()
                .AddDistanceToTargetReached()
                .AddExplosionDamage(new ReactiveVariable<float>(config.ExplosionDamage))
                .AddSpawnInitialTime(new ReactiveVariable<float>(config.SpawnProcessTime))
                .AddSpawnCurrentTime()
                .AddInSpawnProcess()
                .AddInDetonateProcess()
                .AddStartTauntEvent()
                .AddTauntFinishedEvent()
                .AddDetonateTauntIndex()
                .AddDetonateProcessCurrentTime()
                .AddInExplosionProcess()
                .AddStartExplosionEvent()
                .AddExplosionCurrentTime()
                .AddHideExplosionSourceEvent()
                .AddHideExplosionSourceDelayTime(new ReactiveVariable<float>(config.HideExplosionSourceDelayTime))
                .AddEnemySpawnOrigin(position)
                .AddEnemyHitStunRemainingTime(new ReactiveVariable<float>(0f));
            
            ICompositeCondition canMove = new CompositeCondition()
                .Add(new FuncCondition(() => entity.IsDead.Value == false))
                .Add(new FuncCondition(() => entity.InSpawnProcess.Value == false))
                .Add(new FuncCondition(() => entity.DistanceToTargetReached.Value == false))
                .Add(new FuncCondition(() => entity.EnemyHitStunRemainingTime.Value <= 0f));

            ICompositeCondition canRotate = new CompositeCondition()
                .Add(new FuncCondition(() => entity.IsDead.Value == false))
                .Add(new FuncCondition(() => entity.InSpawnProcess.Value == false))
                .Add(new FuncCondition(() => entity.DistanceToTargetReached.Value == false))
                .Add(new FuncCondition(() => entity.EnemyHitStunRemainingTime.Value <= 0f));

            ICompositeCondition mustDie = new CompositeCondition(LogicOperations.Or)
                .Add(new FuncCondition(() => entity.CurrentHealth.Value <= 0));

            ICompositeCondition mustSelfRelease = new CompositeCondition()
                .Add(new FuncCondition(() => entity.IsDead.Value))
                .Add(new FuncCondition(() => entity.InDeathProcess.Value == false));

            ICompositeCondition canTakeDamage = new CompositeCondition()
                .Add(new FuncCondition(() => entity.InSpawnProcess.Value == false))
                .Add(new FuncCondition(() => entity.IsDead.Value == false));

            ICompositeCondition canDetonate = new CompositeCondition()
                .Add(new FuncCondition(() => entity.IsDead.Value == false))
                .Add(new FuncCondition(() => entity.InDetonateProcess.Value == false))
                .Add(new FuncCondition(() => entity.InSpawnProcess.Value == false));
            
            entity
                .AddCanMove(canMove)
                .AddCanRotate(canRotate)
                .AddCanStartDetonate(canDetonate)
                .AddMustDie(mustDie)
                .AddMustSelfRelease(mustSelfRelease)
                .AddCanTakeDamage(canTakeDamage);

            entity
                .AddSystem(new SpawnProcessTimerSystem())
                .AddSystem(new RigidbodyMovementSystem())
                .AddSystem(new RigidbodyRotationSystem())
                .AddSystem(new EnemyHitReactionSystem())
                .AddSystem(new TakeDamageSystem())
                .AddSystem(new DistanceDetectorSystem())
                .AddSystem(new StartDetonationSequence())
                .AddSystem(new DetonationProcessTimerSystem())
                .AddSystem(new TauntProcessSystem())
                .AddSystem(new StartExplosionSystem())
                .AddSystem(new ExplosionProcessTimerSystem())
                .AddSystem(new ExplosionDelayEndTriggerSystem())
                .AddSystem(new ExplosionDamageOnDetonationSystem())
                .AddSystem(new DeathSystem())
                .AddSystem(new DisableCollidersOnDeathSystem())
                .AddSystem(new DeathProcessTimerSystem())
                .AddSystem(new SelfReleaseSystem(_entitiesLifeContext));

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

            SpellcoreCombatConfig spellcoreCombatConfig =
                _container.Resolve<ConfigsProviderService>().GetConfig<SpellcoreCombatConfig>();

            entity
                .AddAreaImpactDamage(new ReactiveVariable<float>(spellcoreCombatConfig.MineDamagePerPulse))
                .AddDealAreaImpactDamageRequest();

            entity.AddSystem(new MineFactoryPulseDetonationSystem(
                _container.Resolve<SectorEnemyQueryService>(),
                _container.Resolve<PlantDamageApplicationService>(),
                _container.Resolve<GameplayJuiceService>(),
                _container.Resolve<SectorRegistryService>(),
                spellcoreCombatConfig,
                _container.Resolve<PlantBuildingBuffJuiceService>()));

            return entity;
        }
        
        public Entity CreateTurret(Vector3 position, TurretConfig turretConfig)
        {
            Entity entity = CreateEmpty();
            
            _monoEntitiesFactory.Create(entity, position, turretConfig.PrefabPath);

            entity
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
                .AddSystem(new PlantTurretInstantShootSystem(
                    this,
                    _container.Resolve<SectorEnemyQueryService>(),
                    _container.Resolve<PlantDamageApplicationService>(),
                    _container.Resolve<PlantDamageCounterService>(),
                    _container.Resolve<GameplayJuiceService>(),
                    _container.Resolve<PlantBuildingBuffService>(),
                    _container.Resolve<PlantBuildingBuffJuiceService>()))
                .AddSystem(new EndAttackSystem())
                .AddSystem(new AttackCooldownTimerSystem());

            entity.AddSystem(new TurretCombatTargetRefreshSystem(
                _container.Resolve<SectorEnemyQueryService>()));

            return entity;
        }
        
        public Entity CreateToxicArea(Vector3 position, ToxicAreaConfig toxicAreaConfig)
        {
            Entity entity = CreateEmpty();
            
            _monoEntitiesFactory.Create(entity, position, toxicAreaConfig.PrefabPath);

            entity
                .AddDamagePerTick(new ReactiveVariable<float>(toxicAreaConfig.DamagePerTick))
                .AddDamageInterval(new ReactiveVariable<float>(toxicAreaConfig.DamageInterval))
                .AddDamageTimer(new ReactiveVariable<float>(toxicAreaConfig.DamageInterval));

            SpellcoreCombatConfig spellcoreCombatConfig =
                _container.Resolve<ConfigsProviderService>().GetConfig<SpellcoreCombatConfig>();
            float slowMoveSpeedFraction = spellcoreCombatConfig.ToxicSlowMoveSpeedFraction;
            
            entity
                .AddSystem(new ToxicSectorCombatSystem(
                    _container.Resolve<SectorEnemyQueryService>(),
                    _container.Resolve<PlantDamageApplicationService>(),
                    _container.Resolve<PlantDamageCounterService>(),
                    _container.Resolve<GameplayJuiceService>(),
                    _container.Resolve<PlantBuildingBuffJuiceService>(),
                    slowMoveSpeedFraction,
                    toxicAreaConfig.SlowAuraPrefab,
                    toxicAreaConfig.SlowAuraBaseScale,
                    toxicAreaConfig.SlowAuraLocalPositionOffset,
                    toxicAreaConfig.SlowAuraLocalScaleMultiplier));

            return entity;
        }
        
        public Entity CreateProjectile(Vector3 position, Vector3 direction, float damage, Entity owner)
        {
            return CreateProjectile(position, direction, damage, 25f, owner, TakeDamageVisualKind.Default);
        }

        public Entity CreateProjectile(
            Vector3 position,
            Vector3 direction,
            float damage,
            float speed,
            Entity owner,
            TakeDamageVisualKind visualKind = TakeDamageVisualKind.Default)
        {
            Entity entity = CreateEmpty();

            _monoEntitiesFactory.Create(entity, position, "Entities/Projectile");

            entity
                .AddMoveDirection(new ReactiveVariable<Vector3>(direction))
                .AddMoveSpeed(new ReactiveVariable<float>(speed))
                .AddIsMoving()
                .AddRotationDirection(new ReactiveVariable<Vector3>(direction))
                .AddRotationSpeed(new ReactiveVariable<float>(9999))
                .AddIsDead()
                .AddContactsDetectingMask(Layers.CharactersMask | Layers.EnviromentMask)
                .AddContactCollidersBuffer(new Buffer<Collider>(64))
                .AddContactEntitiesBuffer(new Buffer<Entity>(64))
                .AddBodyContactDamage(new ReactiveVariable<float>(damage))
                .AddComponent(new ContactDamageVisualKind { Value = visualKind })
                .AddComponent(new ContactDamageOwner { Value = owner })
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
                .AddSystem(new DealDamageOnContactSystem(
                    _container.Resolve<GameplayJuiceService>(),
                    _container.Resolve<PlantBuildingBuffJuiceService>()))
                .AddSystem(new DeathMaskTouchDetectorSystem())
                .AddSystem(new AnotherTeamTouchDetectorSystem())
                .AddSystem(new ProjectileOffScreenBoundsSystem(
                    _container.Resolve<MouseRaycastService>().Camera,
                    _container.Resolve<SectorRegistryService>(),
                    _container.Resolve<ConfigsProviderService>().GetConfig<ProjectileBoundsConfig>()))
                .AddSystem(new DeathSystem())
                .AddSystem(new DisableCollidersOnDeathSystem())
                .AddSystem(new SelfReleaseSystem(_entitiesLifeContext));

            _entitiesLifeContext.Add(entity);

            return entity;
        }

        public Entity CreateEssencePickup(Vector3 position, int amount, EssenceConfig essenceConfig)
        {
            Entity entity = CreateEmpty();

            Vector3 spawnPosition = position;
            spawnPosition.y += essenceConfig.PickupFloorOffset;

            MonoEntity monoEntity = _monoEntitiesFactory.Create(entity, spawnPosition, "Entities/EssencePickup");

            EnsureEssencePickupVisual(monoEntity, essenceConfig);
            ConfigureEssenceHoverCollider(entity, essenceConfig);

            entity
                .AddEssenceAmount(new ReactiveVariable<int>(amount))
                .AddEssenceHoverUnlockRemainingTime(new ReactiveVariable<float>(essenceConfig.HoverUnlockDelay))
                .AddEssenceCanAcceptHover(new ReactiveVariable<bool>(false))
                .AddEssenceIsVacuuming(new ReactiveVariable<bool>(false))
                .AddEssenceIsCollected(new ReactiveVariable<bool>(false))
                .AddEssenceStartVacuumRequest()
                .AddEssenceHoverReadyEvent()
                .AddEssenceVacuumStartedEvent()
                .AddEssenceCollectedEvent();

            ICompositeCondition mustSelfRelease = new CompositeCondition()
                .Add(new FuncCondition(() => entity.EssenceIsCollected.Value));

            entity.AddMustSelfRelease(mustSelfRelease);

            entity
                .AddSystem(new EssenceHoverUnlockSystem())
                .AddSystem(new EssenceVacuumSystem(
                    essenceConfig,
                    _container.Resolve<RunEssenceService>(),
                    _mainHeroHolderService))
                .AddSystem(new SelfReleaseSystem(_entitiesLifeContext));

            _entitiesLifeContext.Add(entity);

            return entity;
        }

        private void EnsureEssencePickupVisual(MonoEntity monoEntity, EssenceConfig essenceConfig)
        {
            Transform rootTransform = monoEntity.transform;

            if (rootTransform.childCount == 0)
            {
                if (essenceConfig.PickupGlowPrefab == null)
                {
                    Debug.LogError("EssenceConfig.PickupGlowPrefab is not assigned.");
                    return;
                }

                GameObject glowInstance = Object.Instantiate(essenceConfig.PickupGlowPrefab, rootTransform);
                glowInstance.transform.localPosition = Vector3.zero;
                glowInstance.transform.localRotation = Quaternion.identity;
                glowInstance.transform.localScale = Vector3.one;

                Collider[] glowColliders = glowInstance.GetComponentsInChildren<Collider>(true);

                for (int index = 0; index < glowColliders.Length; index++)
                    Object.Destroy(glowColliders[index]);
            }

            Transform visualRoot = rootTransform.GetChild(0);
            visualRoot.localScale = Vector3.one * essenceConfig.PickupGlowGroundScale;

            ParticleSystem[] particleSystems = visualRoot.GetComponentsInChildren<ParticleSystem>(true);

            for (int index = 0; index < particleSystems.Length; index++)
            {
                ParticleSystem.MainModule mainModule = particleSystems[index].main;
                mainModule.scalingMode = ParticleSystemScalingMode.Hierarchy;
                particleSystems[index].Play(true);
            }
        }

        private static void ConfigureEssenceHoverCollider(Entity entity, EssenceConfig essenceConfig)
        {
            if (entity.TryGetEssenceHoverCollider(out Collider hoverCollider) == false)
                return;

            if (hoverCollider is SphereCollider sphereCollider)
            {
                sphereCollider.center = new Vector3(0f, essenceConfig.PickupHoverColliderCenterY, 0f);
                sphereCollider.radius = essenceConfig.PickupHoverColliderRadius;
                sphereCollider.isTrigger = true;
            }

            hoverCollider.enabled = false;
        }

        private Entity CreateEmpty() => new Entity();

        private float ResolveEnemyMoveSpeed(float configMoveSpeed)
        {
            SpellcoreCombatConfig spellcoreCombatConfig =
                _container.Resolve<ConfigsProviderService>().GetConfig<SpellcoreCombatConfig>();

            return configMoveSpeed * spellcoreCombatConfig.EnemyMoveSpeedScale;
        }
    }
}
