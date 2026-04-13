using _Project.Develop.Runtime.Configs.Gameplay.MouseActions;
using _Project.Develop.Runtime.Gameplay.Features.Input;
using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;
using Assets._Project.Develop.Runtime.Utilities;
using Assets._Project.Develop.Runtime.Utilities.ConfigsManagment;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.Actions
{
    public class MouseClickActions
    {
        private readonly GameplayActionSetService _actionSetService;
        private readonly CombatClick _combatClick;
        private readonly PeacefulClick _peacefulClick;
        private readonly MouseInput _mouseInput;
        private readonly MouseRaycastService _mouseRaycastService;
        private readonly MouseActionsConfig  _mouseActionsConfig;

        private int _clickLayerMask;
        private RaycastHit _raycastHit;


        public MouseClickActions(
            GameplayActionSetService actionSetService,
            CombatClick combatClick,
            PeacefulClick peacefulClick,
            MouseInput mouseInput,
            MouseRaycastService mouseRaycastService,
            ConfigsProviderService configsProviderService)
        {
            _actionSetService = actionSetService;
            _combatClick = combatClick;
            _peacefulClick = peacefulClick;
            _mouseInput = mouseInput;
            _mouseRaycastService = mouseRaycastService;
            _mouseActionsConfig = configsProviderService.GetConfig<MouseActionsConfig>();
        }

        public void Update(float deltaTime)
        {
            ActionSet actionMode = _actionSetService.CurrentActionSet.Value;
            
            if (TryGetClick())
            {
                if (actionMode == ActionSet.Combat)
                {
                    if (_mouseInput.FireButtonPressed)
                        _combatClick.TryPerformClick(_raycastHit, _mouseActionsConfig);
                }
                else
                {
                    bool isFloorFirstLayer = _raycastHit.collider.gameObject.layer == Layers.FloorLayerIndex;
                    
                    if (_mouseInput.FireButtonPressed && isFloorFirstLayer)
                        _peacefulClick.TryPerformClick(_raycastHit);
                }
            }
        }

        private bool TryGetClick()
        {
            if (_mouseRaycastService.TryGetHit(
                    _mouseInput.PointerScreenPosition, 
                    out RaycastHit hit, 
                    _mouseActionsConfig.MouseRaycastDistance,
                    Layers.GenericLayerMask))
            {
                _raycastHit = hit;
                return true;
            }

            return false;
        }
    }
}