using DG.Tweening;
using UnityEngine;

namespace _Project.Develop.Runtime.UI.Gameplay.GnomeKillToast
{
    public class GnomeKillToastView : MonoBehaviour
    {
        private const float SHOW_DURATION_SECONDS = 0.25f;
        private const float VISIBLE_DURATION_SECONDS = 2f;
        private const float HIDE_DURATION_SECONDS = 0.25f;
        private const float HIDDEN_OFFSET_X = 120f;
        private const float HIDDEN_OFFSET_Y = -120f;

        [SerializeField] private RectTransform _body;
        [SerializeField] private CanvasGroup _canvasGroup;

        private Vector2 _shownAnchoredPosition;
        private Vector2 _hiddenAnchoredPosition;
        private Sequence _sequence;
        private bool _isInitialized;

        private void EnsureInitialized()
        {
            if (_isInitialized == true)
                return;

            _isInitialized = true;
            _shownAnchoredPosition = _body.anchoredPosition;
            _hiddenAnchoredPosition = _shownAnchoredPosition + new Vector2(HIDDEN_OFFSET_X, HIDDEN_OFFSET_Y);

            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        public void Play()
        {
            EnsureInitialized();
            _sequence?.Kill();

            gameObject.SetActive(true);
            _body.anchoredPosition = _hiddenAnchoredPosition;
            _canvasGroup.alpha = 0f;

            _sequence = DOTween.Sequence()
                .Append(_body.DOAnchorPos(_shownAnchoredPosition, SHOW_DURATION_SECONDS).SetEase(Ease.OutCubic))
                .Join(_canvasGroup.DOFade(1f, SHOW_DURATION_SECONDS))
                .AppendInterval(VISIBLE_DURATION_SECONDS)
                .Append(_body.DOAnchorPos(_hiddenAnchoredPosition, HIDE_DURATION_SECONDS).SetEase(Ease.InCubic))
                .Join(_canvasGroup.DOFade(0f, HIDE_DURATION_SECONDS))
                .OnComplete(HideInstant)
                .SetUpdate(true)
                .Play();
        }

        private void HideInstant()
        {
            _sequence?.Kill();
            _canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            _sequence?.Kill();
        }
    }
}
