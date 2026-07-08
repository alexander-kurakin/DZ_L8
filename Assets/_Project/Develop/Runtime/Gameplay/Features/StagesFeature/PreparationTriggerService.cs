using Assets._Project.Develop.Runtime.Utilities.Reactive;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature
{
    public class PreparationTriggerService
    {
        private readonly ReactiveVariable<bool> _prepareTriggerClicked = new();

        public IReadOnlyVariable<bool> PrepareTriggerClicked => _prepareTriggerClicked;

        public void Create()
        {
            _prepareTriggerClicked.Value = false;
        }

        public void Cleanup()
        {
            _prepareTriggerClicked.Value = false;
        }

        public void Update(float deltaTime)
        {
        }

        public void Trigger()
        {
            _prepareTriggerClicked.Value = true;
        }
    }
}
