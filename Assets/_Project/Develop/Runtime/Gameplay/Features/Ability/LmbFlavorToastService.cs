using Assets._Project.Develop.Runtime.Utilities.Reactive;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Ability
{
    public class LmbFlavorToastService
    {
        private readonly ReactiveEvent<LmbFlavorToastType> _toastRequested = new();

        public ReactiveEvent<LmbFlavorToastType> ToastRequested => _toastRequested;

        public void Show(LmbFlavorToastType toastType)
        {
            _toastRequested.Invoke(toastType);
        }
    }
}
