using System;
using Assets._Project.Develop.Runtime.Gameplay.Features.Ability;
using Assets._Project.Develop.Runtime.UI.Core;
using UnityEngine;

namespace _Project.Develop.Runtime.UI.Gameplay.LmbFlavorToast
{
    public class LmbFlavorToastPresenter : IPresenter
    {
        private const string TANK_ARMOR_MESSAGE = "Aw no, spells do not penetrate armor";
        private const string DRAGON_MAGICAL_DEFENSE_MESSAGE = "Aw no, magical defense is too high";

        private readonly LmbFlavorToastService _lmbFlavorToastService;
        private readonly LmbFlavorToastView _view;

        private IDisposable _toastSubscription;

        public LmbFlavorToastPresenter(LmbFlavorToastService lmbFlavorToastService, Transform parent)
        {
            _lmbFlavorToastService = lmbFlavorToastService;

            GameObject toastObject = new GameObject(nameof(LmbFlavorToastView));
            toastObject.transform.SetParent(parent, false);
            _view = toastObject.AddComponent<LmbFlavorToastView>();
            _view.Initialize();
        }

        public void Initialize()
        {
            _toastSubscription = _lmbFlavorToastService.ToastRequested.Subscribe(OnToastRequested);
        }

        private void OnToastRequested(LmbFlavorToastType toastType)
        {
            switch (toastType)
            {
                case LmbFlavorToastType.TankArmor:
                    _view.Show(TANK_ARMOR_MESSAGE);
                    break;

                case LmbFlavorToastType.DragonMagicalDefense:
                    _view.Show(DRAGON_MAGICAL_DEFENSE_MESSAGE);
                    break;
            }
        }

        public void Dispose()
        {
            _toastSubscription?.Dispose();

            if (_view != null)
                UnityEngine.Object.Destroy(_view.gameObject);
        }
    }
}
