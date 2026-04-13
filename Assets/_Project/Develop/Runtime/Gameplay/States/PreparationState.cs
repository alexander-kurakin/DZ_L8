using _Project.Develop.Runtime.Configs.Gameplay.MouseActions;
using _Project.Develop.Runtime.Gameplay.Features.Actions;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature;
using Assets._Project.Develop.Runtime.Utilities.ConfigsManagment;
using Assets._Project.Develop.Runtime.Utilities.StateMachineCore;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.States
{
    public class PreparationState : State, IUpdatableState
    {
        private readonly PreparationTriggerService _preparationTriggerService;
        private readonly GameplayActionSetService _actionSetService;
        private readonly ContactTriggerConfig _contactTriggerConfig;

        public PreparationState(
            PreparationTriggerService preparationTriggerService, 
            GameplayActionSetService actionSetService,
            ConfigsProviderService  configsProviderService)
        {
            _preparationTriggerService = preparationTriggerService;
            _actionSetService = actionSetService;
            _contactTriggerConfig = configsProviderService.GetConfig<ContactTriggerConfig>();
        }

        public override void Enter()
        {
            base.Enter();
            
            _actionSetService.SetActionSet(ActionSet.Peaceful);

            _preparationTriggerService.Create(_contactTriggerConfig.ContactTriggerStartPosition);
        }

        public void Update(float deltaTime)
        {
            _preparationTriggerService.Update(deltaTime);
        }

        public override void Exit()
        {
            base.Exit();

            _preparationTriggerService.Cleanup();
        }
    }
}
