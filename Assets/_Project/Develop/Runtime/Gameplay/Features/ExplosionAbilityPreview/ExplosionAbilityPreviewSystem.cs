using _Project.Develop.Runtime.Gameplay.Features.Input;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Gameplay.Features.GameplayStateBridge;
using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.ExplosionAbilityPreview
{
    public class ExplosionAbilityPreviewSystem : IInitializableSystem, IUpdatableSystem
    {
        private readonly IMouseInputService _mouseInput;
        private readonly IMouseRaycastService _mouseRaycast;
        private readonly SectorRegistryService _sectorRegistryService;
        private readonly ExplodeAtPointAbilityConfig _config;

        private ReactiveVariable<GameplayStates> _gameplayPhase;
        private ReactiveVariable<Vector3> _previewWorldPoint;
        private ReactiveVariable<bool> _previewVisible;
        private ReactiveVariable<float> _previewIndicatorDiameter;

        public ExplosionAbilityPreviewSystem(
            IMouseInputService mouseInput,
            IMouseRaycastService mouseRaycast,
            SectorRegistryService sectorRegistryService,
            ExplodeAtPointAbilityConfig config)
        {
            _mouseInput = mouseInput;
            _mouseRaycast = mouseRaycast;
            _sectorRegistryService = sectorRegistryService;
            _config = config;
        }

        public void OnInit(Entity entity)
        {
            _gameplayPhase = entity.GameplayPhase;
            _previewWorldPoint = entity.ExplosionPreviewWorldPoint;
            _previewVisible = entity.ExplosionPreviewVisible;
            _previewIndicatorDiameter = entity.ExplosionPreviewIndicatorDiameter;
            _previewIndicatorDiameter.Value = _config.PreviewIndicatorDiameter;
        }

        public void OnUpdate(float deltaTime)
        {
            _previewVisible.Value = _gameplayPhase.Value == GameplayStates.Preparation
                                    || _gameplayPhase.Value == GameplayStates.StageProcess;

            if (_previewVisible.Value == false)
                return;

            if (SectorSurfaceClickUtility.TryGetHorizontalPlanePoint(
                    _mouseRaycast,
                    _mouseInput.PointerScreenPosition,
                    _sectorRegistryService,
                    out Vector3 planePoint) == false)
            {
                return;
            }

            planePoint.y = _sectorRegistryService.Center.y + _sectorRegistryService.GridConfig.SectorSurfaceGroundYOffset;
            _previewWorldPoint.Value = planePoint;
        }
    }
}
