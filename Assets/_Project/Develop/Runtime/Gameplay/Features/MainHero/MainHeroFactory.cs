using Assets._Project.Develop.Runtime.Configs.Gameplay.Camera;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Throw;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.AI;
using Assets._Project.Develop.Runtime.Gameplay.Features.CameraFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.MovementFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.TeamsFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.ThrowFeature;
using Assets._Project.Develop.Runtime.Infrastructure.DI;
using Assets._Project.Develop.Runtime.Utilities.AssetsManagment;
using Assets._Project.Develop.Runtime.Utilities.Conditions;
using Assets._Project.Develop.Runtime.Utilities.ConfigsManagment;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System.Collections.Generic;
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
        private readonly IMouseRaycastService _mouseRaycastService;
        private readonly IInputService _desktopInput;
        private readonly BrainsFactory _brainsFactory;
        private readonly ResourcesAssetsLoader _resourcesAssetsLoader;
        
        private int _currentLevelNumber;

        public MainHeroFactory(DIContainer container, int currentLevelNumber)
        {
            _container = container;
            _entitiesFactory = _container.Resolve<EntitiesFactory>();
            _configsProviderService = _container.Resolve<ConfigsProviderService>();
            _entitiesLifeContext = _container.Resolve<EntitiesLifeContext>();
            _mainHeroHolderService = _container.Resolve<MainHeroHolderService>();
			_mouseInput = _container.Resolve<IMouseInputService>();
            _mouseRaycastService = _container.Resolve<IMouseRaycastService>();
            _desktopInput = _container.Resolve<IInputService>();
            _brainsFactory = _container.Resolve<BrainsFactory>();
            _resourcesAssetsLoader = _container.Resolve<ResourcesAssetsLoader>();
            
            _currentLevelNumber =  currentLevelNumber;
        }

        public Entity Create()
        {
            HeroConfig heroConfig = _configsProviderService.GetConfig<HeroConfig>();
            ThrowChargeConfig throwChargeConfig = _configsProviderService.GetConfig<ThrowChargeConfig>();
            HeroCameraConfig heroCameraConfig = _configsProviderService.GetConfig<HeroCameraConfig>();

            Entity entity = _entitiesFactory.CreateMainHero(heroConfig);

            entity
                .AddMoveDirection()
                .AddMoveSpeed(new ReactiveVariable<float>(6))
                .AddIsMoving()
                .AddRotationDirection()
                .AddRotationSpeed(new ReactiveVariable<float>(450))
                .AddIsMainHero()
                .AddTeam(new ReactiveVariable<Teams>(Teams.MainHero))
                .AddCurrentProjectile()
                .AddIsProjectileInHand()
                .AddIsChargingThrow()
                .AddThrowChargePower()
                .AddThrowReleased();

            entity.AddComponent(new ThrowTrajectoryPreview()
            {
                TrajectoryPoints = new List<ReactiveVariable<Vector3>>(),
                IsVisible = new ReactiveVariable<bool>(false)
            });

            ApplyCameraAlignedStartRotation(entity);

            ICompositeCondition canChargeThrow = new CompositeCondition()
                .Add(new FuncCondition(() => entity.IsProjectileInHand.Value == true));

            ICompositeCondition canReleaseThrow = new CompositeCondition()
                .Add(new FuncCondition(() => entity.IsChargingThrow.Value == true))
                .Add(new FuncCondition(() => entity.IsProjectileInHand.Value == true));

            entity
                .AddCanChargeThrow(canChargeThrow)
                .AddCanReleaseThrow(canReleaseThrow);

            ICompositeCondition canMove = new CompositeCondition()
                .Add(new FuncCondition(() => entity.IsChargingThrow.Value == false));

            ICompositeCondition canRotate = new CompositeCondition()
                .Add(new FuncCondition(() => entity.IsChargingThrow.Value == false));

            entity
                .AddCanMove(canMove)
                .AddCanRotate(canRotate);

            entity
                .AddSystem(new ThrowChargeSystem(_mouseInput, throwChargeConfig))
                .AddSystem(new ThrowTrajectoryPreviewSystem(_mouseInput, _mouseRaycastService, throwChargeConfig))
                .AddSystem(new ThrowReleaseSystem(_mouseInput, _mouseRaycastService, throwChargeConfig))
                .AddSystem(new ProjectileLaunchSystem(throwChargeConfig))
                .AddSystem(new ThrowAimMarkerSystem(_resourcesAssetsLoader))
                .AddSystem(new ProjectileRespawnSystem(_entitiesFactory, throwChargeConfig))
                .AddSystem(new HeroCameraSystem(heroCameraConfig))
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
