using _Project.Develop.Runtime.Gameplay.Features.Input;
using Assets._Project.Develop.Runtime.Configs.Gameplay.MouseConfig;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Gameplay.Features.GameplayStateBridge;
using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature;
using Assets._Project.Develop.Runtime.Utilities;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.ExplosionAbilityPreview
{
    public class ExplosionAbilityPreviewSystem : IInitializableSystem, IUpdatableSystem
    {
        private const float PREVIEW_INDICATOR_DIAMETER = 10f;
        private const float PREVIEW_GROUND_Y_OFFSET = 0.25f;

        private readonly IMouseInputService _mouseInput;
        private readonly IMouseRaycastService _mouseRaycast;
        private readonly RaycastConfig _raycastConfig;
        private readonly SectorRegistryService _sectorRegistryService;

        private ReactiveVariable<GameplayStates> _gameplayPhase;
        private ReactiveVariable<Vector3> _previewWorldPoint;
        private ReactiveVariable<bool> _previewVisible;
        private ReactiveVariable<float> _previewIndicatorDiameter;

        public ExplosionAbilityPreviewSystem(
            IMouseInputService mouseInput,
            IMouseRaycastService mouseRaycast,
            RaycastConfig raycastConfig,
            SectorRegistryService sectorRegistryService)
        {
            _mouseInput = mouseInput;
            _mouseRaycast = mouseRaycast;
            _raycastConfig = raycastConfig;
            _sectorRegistryService = sectorRegistryService;
        }

        public void OnInit(Entity entity)
        {
            _gameplayPhase = entity.GameplayPhase;
            _previewWorldPoint = entity.ExplosionPreviewWorldPoint;
            _previewVisible = entity.ExplosionPreviewVisible;
            _previewIndicatorDiameter = entity.ExplosionPreviewIndicatorDiameter;
            _previewIndicatorDiameter.Value = PREVIEW_INDICATOR_DIAMETER;
        }

        public void OnUpdate(float deltaTime)
        {
            _previewVisible.Value = _gameplayPhase.Value == GameplayStates.Preparation
                                    || _gameplayPhase.Value == GameplayStates.StageProcess;

            if (_previewVisible.Value == false)
                return;

            if (TryGetGroundPointUnderCursor(out Vector3 groundPoint))
                _previewWorldPoint.Value = groundPoint;
        }

        private bool TryGetGroundPointUnderCursor(out Vector3 groundPoint)
        {
            if (TryGetCursorHitPoint(out Vector3 hitPoint) == false)
            {
                groundPoint = default;
                return false;
            }

            float previewPlaneY = GetPreviewPlaneY();
            groundPoint = new Vector3(hitPoint.x, previewPlaneY, hitPoint.z);
            return true;
        }

        private bool TryGetCursorHitPoint(out Vector3 hitPoint)
        {
            Vector2 pointerScreenPosition = _mouseInput.PointerScreenPosition;
            float raycastDistance = _raycastConfig.MouseRaycastDistance;

            if (_mouseRaycast.TryGetHit(
                    pointerScreenPosition,
                    out RaycastHit floorHit,
                    raycastDistance,
                    Layers.FloorLayerMask))
            {
                hitPoint = floorHit.point;
                return true;
            }

            if (_mouseRaycast.TryGetHit(
                    pointerScreenPosition,
                    out RaycastHit genericHit,
                    raycastDistance,
                    Layers.GenericLayerMask))
            {
                hitPoint = genericHit.point;
                return true;
            }

            hitPoint = default;
            return false;
        }

        private float GetPreviewPlaneY()
        {
            if (_sectorRegistryService.IsInitialized)
                return _sectorRegistryService.Center.y + PREVIEW_GROUND_Y_OFFSET;

            return PREVIEW_GROUND_Y_OFFSET;
        }
    }
}
