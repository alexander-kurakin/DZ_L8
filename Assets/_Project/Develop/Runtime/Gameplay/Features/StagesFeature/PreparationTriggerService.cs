using Assets._Project.Develop.Runtime.Utilities.Reactive;
using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature
{
    public class PreparationTriggerService
    {
        private readonly ReactiveVariable<bool> _prepareTriggerClicked = new();
        private readonly IMouseInputService _mouseInputService;

        public PreparationTriggerService(IMouseInputService mouseInputService)
        {
            _mouseInputService = mouseInputService;
        }

        public IReadOnlyVariable<bool> PrepareTriggerClicked => _prepareTriggerClicked;

        public void Create()
        {
            _prepareTriggerClicked.Value = false;
        }

        public void Update(float deltaTime)
        {
            if (_prepareTriggerClicked.Value == false && _mouseInputService.FireButtonPressed)
                _prepareTriggerClicked.Value = true;
        }

        public void Cleanup()
        {
            _prepareTriggerClicked.Value = false;
        }
    }
}
