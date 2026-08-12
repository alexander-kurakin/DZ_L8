using Assets._Project.Develop.Runtime.UI.Core;
using DG.Tweening;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.UI.Gameplay.CoachHintArrows
{
    public class CoachHintArrowPresenter : IPresenter
    {
        private const float PULSE_SCALE = 1.18f;
        private const float PULSE_DURATION_SECONDS = 0.45f;

        private readonly CoachHintArrowView _view;
        private Tween _pulseTween;

        public CoachHintArrowPresenter(CoachHintArrowView view)
        {
            _view = view;
        }

        public CoachHintArrowView View => _view;

        public void Initialize()
        {
            Initialize(CoachHintArrowView.PointDownZDegrees);
        }

        public void Initialize(float rotationZDegrees)
        {
            _pulseTween?.Kill();

            _view.SetRotationZ(rotationZDegrees);
            _view.RectTransform.localScale = Vector3.one;

            _pulseTween = _view.RectTransform
                .DOScale(PULSE_SCALE, PULSE_DURATION_SECONDS)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetUpdate(true)
                .Play();
        }

        public void Dispose()
        {
            _pulseTween?.Kill();
            _pulseTween = null;
        }
    }
}
