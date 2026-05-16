using _Project.Develop.Runtime.Gameplay.Features.Input;
using Assets._Project.Develop.Runtime.Configs.Gameplay.MouseConfig;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Gameplay.Features.Ability;
using Assets._Project.Develop.Runtime.Gameplay.Features.GameplayStateBridge;
using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;
using Assets._Project.Develop.Runtime.Utilities;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.ExplosionAbilityPreview
{
    public class ExplosionAbilityPreviewSystem : IInitializableSystem, IUpdatableSystem
    {
        private readonly IMouseInputService _mouseInput;
        private readonly IMouseRaycastService _mouseRaycast;
        private readonly RaycastConfig _raycastConfig;

        private ReactiveVariable<GameplayStates> _gameplayPhase;
        private ReactiveVariable<AbilityType> _activeAbility;
        private ReactiveVariable<Vector3> _previewWorldPoint;
        private ReactiveVariable<bool> _previewVisible;

        public ExplosionAbilityPreviewSystem(
            IMouseInputService mouseInput,
            IMouseRaycastService mouseRaycast,
            RaycastConfig raycastConfig)
        {
            _mouseInput = mouseInput;
            _mouseRaycast = mouseRaycast;
            _raycastConfig = raycastConfig;
        }

        public void OnInit(Entity entity)
        {
            _gameplayPhase = entity.GameplayPhase;
            _activeAbility = entity.AbilityUserActiveAbility;
            _previewWorldPoint = entity.ExplosionPreviewWorldPoint;
            _previewVisible = entity.ExplosionPreviewVisible;
        }

        public void OnUpdate(float deltaTime)
        {
            _previewVisible.Value = _gameplayPhase.Value == GameplayStates.StageProcess && _activeAbility.Value == AbilityType.ExplodeAtPoint;
            
            if (_previewVisible.Value == false)
                return;
            
            if (TryGetGroundPointUnderCursor(out Vector3 groundPoint))
                _previewWorldPoint.Value = groundPoint;
        }

        private bool TryGetGroundPointUnderCursor(out Vector3 groundPoint)
        {
            if (_mouseRaycast.TryGetHit(
                    _mouseInput.PointerScreenPosition,
                    out RaycastHit hit,
                    _raycastConfig.MouseRaycastDistance,
                    Layers.GenericLayerMask))
            {
                groundPoint = hit.point;
                return true;
            }

            groundPoint = default;
            return false;
        }
    }
}