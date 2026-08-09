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
        private const string FREE_MINE_HINT_MESSAGE = "You get one free mine. Place it anywhere";
        private const string FREE_MINE_SOLD_MESSAGE = "Banger. That was the only one. Better luck next time";
        private const string WAVE_TWO_TANKS_HINT_MESSAGE = "Tanks might need more mines and turret shots than you think. And they are immune to poison";
        private const string DRAGON_POISON_IMMUNE_MESSAGE = "Dragons are immune to poison and mines. Need those turrets to shoot them";
        private const string FIRST_MINE_PLACED_GO_HINT_MESSAGE =
            "Awesome. Now touch the magic sphere and let's get rolling!";
        private const string INVALID_PLACE_HINT_MESSAGE = "You can't park.. em.. place there mate";

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
            _view.transform.SetAsLastSibling();
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

                case LmbFlavorToastType.FreeMineHint:
                    _view.Show(FREE_MINE_HINT_MESSAGE);
                    break;

                case LmbFlavorToastType.FreeMineSold:
                    _view.Show(FREE_MINE_SOLD_MESSAGE);
                    break;

                case LmbFlavorToastType.WaveTwoTanksHint:
                    _view.Show(WAVE_TWO_TANKS_HINT_MESSAGE);
                    break;

                case LmbFlavorToastType.DragonPoisonImmune:
                    _view.Show(DRAGON_POISON_IMMUNE_MESSAGE);
                    break;

                case LmbFlavorToastType.FirstMinePlacedGoHint:
                    _view.Show(FIRST_MINE_PLACED_GO_HINT_MESSAGE);
                    break;

                case LmbFlavorToastType.InvalidPlaceHint:
                    _view.Show(INVALID_PLACE_HINT_MESSAGE);
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
