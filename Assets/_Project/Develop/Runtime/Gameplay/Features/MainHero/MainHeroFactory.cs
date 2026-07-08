using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.AI;
using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.MovementFeature;
using Assets._Project.Develop.Runtime.Infrastructure.DI;
using Assets._Project.Develop.Runtime.Utilities.Conditions;
using Assets._Project.Develop.Runtime.Utilities.ConfigsManagment;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.MainHero
{
    public class MainHeroFactory
    {
        private readonly DIContainer _container;

        private readonly EntitiesFactory _entitiesFactory;
        private readonly ConfigsProviderService _configsProviderService;
        private readonly EntitiesLifeContext _entitiesLifeContext;
        private readonly MainHeroHolderService _mainHeroHolderService;
		private readonly IMouseInputService _mouseInput;
        private readonly IInputService _desktopInput;
        private readonly BrainsFactory _brainsFactory;
        
        private int _currentLevelNumber;

        public MainHeroFactory(DIContainer container, int currentLevelNumber)
        {
            _container = container;
            _entitiesFactory = _container.Resolve<EntitiesFactory>();
            _configsProviderService = _container.Resolve<ConfigsProviderService>();
            _entitiesLifeContext = _container.Resolve<EntitiesLifeContext>();
            _mainHeroHolderService = _container.Resolve<MainHeroHolderService>();
			_mouseInput = _container.Resolve<IMouseInputService>();
            _desktopInput = _container.Resolve<IInputService>();
            _brainsFactory = _container.Resolve<BrainsFactory>();
            
            _currentLevelNumber =  currentLevelNumber;
        }

        public Entity Create()
        {
            HeroConfig heroConfig = _configsProviderService.GetConfig<HeroConfig>();

            Entity entity = _entitiesFactory.CreateMainHero(heroConfig);

            entity
                .AddMoveDirection()
                .AddMoveSpeed(new ReactiveVariable<float>(6))
                .AddIsMoving()
                .AddRotationDirection()
                .AddRotationSpeed(new ReactiveVariable<float>(450))
                .AddIsMainHero();

            ApplyCameraAlignedStartRotation(entity);
            
            ICompositeCondition canMove = new CompositeCondition()
                .Add(new FuncCondition(() => true)); //TBD is not throwing
            
            ICompositeCondition canRotate = new CompositeCondition()
                .Add(new FuncCondition(() => true)); //TBD is not throwing

            entity
                .AddCanMove(canMove)
                .AddCanRotate(canRotate);

            entity
                .AddSystem(new RigidbodyMovementSystem())
                .AddSystem(new RigidbodyRotationSystem());

            _entitiesLifeContext.Add(entity);
            
            _brainsFactory.CreateSimpleMainHeroBrain(entity, _desktopInput);
            
            return entity;
        }

        private static void ApplyCameraAlignedStartRotation(Entity entity)
        {
            Camera camera = Camera.main;

            if (camera == null)
                return;

            Vector3 cameraForward = camera.transform.forward;
            cameraForward.y = 0f;

            if (cameraForward.sqrMagnitude <= 0f)
                return;

            entity.RotationDirection.Value = cameraForward.normalized;
        }
    }
}
