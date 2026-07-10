using Assets._Project.Develop.Runtime.Configs.Gameplay.Gnome;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using Assets._Project.Develop.Runtime.Gameplay.Features.AI;
using Assets._Project.Develop.Runtime.Gameplay.Features.GnomeFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.MovementFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.RunKillCounter;
using Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.TakeDamage;
using Assets._Project.Develop.Runtime.Gameplay.Features.TeamsFeature;
using Assets._Project.Develop.Runtime.Infrastructure.DI;
using Assets._Project.Develop.Runtime.Utilities.Conditions;
using Assets._Project.Develop.Runtime.Utilities.ConfigsManagment;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.GnomeFeature
{
    public class EnemiesFactory
    {
        private readonly EntitiesFactory _entitiesFactory;
        private readonly MonoEntitiesFactory _monoEntitiesFactory;
        private readonly EntitiesLifeContext _entitiesLifeContext;
        private readonly BrainsFactory _brainsFactory;
        private readonly StageProviderService _stageProviderService;
        private readonly GnomeConfig _gnomeConfig;

        public EnemiesFactory(DIContainer container)
        {
            _entitiesFactory = container.Resolve<EntitiesFactory>();
            _monoEntitiesFactory = container.Resolve<MonoEntitiesFactory>();
            _entitiesLifeContext = container.Resolve<EntitiesLifeContext>();
            _brainsFactory = container.Resolve<BrainsFactory>();
            _stageProviderService = container.Resolve<StageProviderService>();
            _gnomeConfig = container.Resolve<ConfigsProviderService>().GetConfig<GnomeConfig>();
        }

        public Entity CreateGnome(GnomePeekPoint peekPoint)
        {
            if (peekPoint == null)
                return null;

            if (peekPoint.IsOccupied == true)
                return null;

            Entity entity = _entitiesFactory.CreateEmpty();
            
            _monoEntitiesFactory.Create(entity, peekPoint.HiddenPosition, peekPoint.HiddenRotation(), _gnomeConfig.PrefabPath);
            ApplyVerticalSpawnOffset(peekPoint, entity);

            float peekOffset = peekPoint.PeekOffset > 0f ? peekPoint.PeekOffset : _gnomeConfig.DefaultPeekOffset;
            ReactiveVariable<bool> isDead = new ReactiveVariable<bool>(false);

            entity
                .AddIsGnome()
                .AddIsPeeking(new ReactiveVariable<bool>(false))
                .AddGnomeHitCount(new ReactiveVariable<int>(0))
                .AddGnomeRequiredHits(_gnomeConfig.RequiredHits)
                .AddGnomePeekOffset(peekOffset)
                .AddGnomePeekDirection(peekPoint.PeekDirection)
                .AddComponent(new GnomePeekLeanAngle
                {
                    Value = peekPoint.IsVerticalLayout == true ? 0f : peekPoint.PeekLeanAngle
                })
                .AddGnomeForcePeekRequested(new ReactiveVariable<bool>(false))
                .AddComponent(new GnomeIsVerticalLayout { Value = peekPoint.IsVerticalLayout })
                .AddGnomeDeathDissolveDuration(_gnomeConfig.DeathDissolveSeconds)
                .AddRunEnemyKillMarker(isDead)
                .AddTeam(new ReactiveVariable<Teams>(Teams.Enemies))
                .AddTakeDamageRequest()
                .AddTakeDamageEvent()
                .AddRotationDirection()
                .AddRotationSpeed(new ReactiveVariable<float>(360f));

            ICompositeCondition canTakeDamage = new CompositeCondition()
                .Add(new FuncCondition(() => entity.IsPeeking.Value == true))
                .Add(new FuncCondition(() => isDead.Value == false));

            ICompositeCondition canRotate = new CompositeCondition()
                .Add(new FuncCondition(() => isDead.Value == false))
                .Add(new FuncCondition(() => peekPoint.IsVerticalLayout == false));

            entity
                .AddCanTakeDamage(canTakeDamage)
                .AddCanRotate(canRotate);

            entity
                .AddSystem(new GnomePeekHitColliderSystem())
                .AddSystem(new GnomeTakeDamageSystem())
                .AddSystem(new RigidbodyRotationSystem());

            _entitiesLifeContext.Add(entity);
            _brainsFactory.CreateGnomePeekBrain(entity, _gnomeConfig);
            _stageProviderService.AddTemporaryEntity(entity);

            if (peekPoint.TryOccupy(entity) == false)
            {
                _entitiesLifeContext.Release(entity);
                return null;
            }

            return entity;
        }

        private static void ApplyVerticalSpawnOffset(GnomePeekPoint peekPoint, Entity entity)
        {
            if (peekPoint.IsVerticalLayout == false)
                return;

            if (entity.TryGetTransform(out Transform gnomeTransform) == false)
                return;

            float gnomeHeight = GetGnomeHeight(gnomeTransform);

            gnomeTransform.position = peekPoint.HiddenPosition - Vector3.up * gnomeHeight * 0.5f;
        }

        private static float GetGnomeHeight(Transform gnomeRoot)
        {
            CapsuleCollider capsule = gnomeRoot.GetComponentInChildren<CapsuleCollider>(true);

            if (capsule == null)
                return 0f;

            return capsule.height * capsule.transform.lossyScale.y;
        }
    }
}
