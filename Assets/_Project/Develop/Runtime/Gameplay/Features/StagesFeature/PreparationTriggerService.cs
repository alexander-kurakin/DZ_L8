using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System;
using _Project.Develop.Runtime.Gameplay.Features.Input;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;
using Assets._Project.Develop.Runtime.Utilities;
using Assets._Project.Develop.Runtime.Utilities.ConfigsManagment;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature
{
    public class PreparationTriggerService
    {
        private ReactiveVariable<bool> _prepareTriggerClicked = new();

        private EntitiesFactory _entitiesFactory;
        private EntitiesLifeContext _entitiesLifeContext;
        private IMouseInputService _mouseInputService;
        private IMouseRaycastService _mouseRaycastService;
        private readonly ContactTriggerConfig _contactTriggerConfig;

        private Entity _nextStageTrigger;

        public PreparationTriggerService(
            EntitiesFactory entitiesFactory, 
            EntitiesLifeContext entitiesLifeContext,
            IMouseInputService mouseInputService,
            IMouseRaycastService mouseRaycastService,
            ConfigsProviderService configsProviderService)
        {
            _entitiesFactory = entitiesFactory;
            _entitiesLifeContext = entitiesLifeContext;
            _mouseInputService = mouseInputService;
            _mouseRaycastService = mouseRaycastService;
            _contactTriggerConfig =  configsProviderService.GetConfig<ContactTriggerConfig>();
        }

        public IReadOnlyVariable<bool> PrepareTriggerClicked => _prepareTriggerClicked;

        public void Create(Vector3 position)
        {
            if (_nextStageTrigger != null)
                throw new InvalidOperationException("Trigger already created");

            _nextStageTrigger = _entitiesFactory.CreateContactTrigger(position, _contactTriggerConfig);
        }

        public void Update(float deltaTime)
        {
            if (_nextStageTrigger != null &&
                _prepareTriggerClicked.Value == false &&
                MouseClickedOnContactTriggerLayer(out RaycastHit hitPoint))
            {
                    _prepareTriggerClicked.Value = 
                        hitPoint.collider != null && hitPoint.collider == _nextStageTrigger.BodyCollider;        
            }
        }
        
        private bool MouseClickedOnContactTriggerLayer(out RaycastHit rayHit)
        {
            if (_mouseInputService.FireButtonPressed)
                if (_mouseRaycastService.TryGetHit(
                        _mouseInputService.PointerScreenPosition,
                        out RaycastHit hit,
                        _contactTriggerConfig.MouseRaycastDistance,
                        Layers.ContactTriggerLayerMask))
                {
                    rayHit = hit;
                    return true;
                }

            rayHit = default;
            return false;
        }

        public void Cleanup()
        {
            _entitiesLifeContext.Release(_nextStageTrigger);
            _prepareTriggerClicked.Value = false;
            _nextStageTrigger = null;
        }
    }
}
