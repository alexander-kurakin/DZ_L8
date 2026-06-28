using Assets._Project.Develop.Runtime.Gameplay.Features.Ability;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace _Project.Develop.Runtime.UI.Gameplay.LmbFlavorToast
{
    public class LmbFlavorToastView : MonoBehaviour
    {
        private const float FADE_IN_DURATION = 0.2f;
        private const float VISIBLE_DURATION = 2.5f;
        private const float FADE_OUT_DURATION = 0.35f;

        private CanvasGroup _canvasGroup;
        private TextMeshProUGUI _label;
        private Sequence _activeSequence;

        public void Initialize()
        {
            RectTransform rectTransform = gameObject.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.2f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.2f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = new Vector2(900f, 80f);

            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            _canvasGroup.alpha = 0f;

            GameObject labelObject = new GameObject("Label");
            labelObject.transform.SetParent(transform, false);

            RectTransform labelRect = labelObject.AddComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            _label = labelObject.AddComponent<TextMeshProUGUI>();
            _label.alignment = TextAlignmentOptions.Center;
            _label.fontSize = 28f;
            _label.color = Color.white;
            _label.enableWordWrapping = true;
        }

        public void Show(string message)
        {
            _label.text = message;

            _activeSequence?.Kill();
            _activeSequence = DOTween.Sequence()
                .Append(_canvasGroup.DOFade(1f, FADE_IN_DURATION))
                .AppendInterval(VISIBLE_DURATION)
                .Append(_canvasGroup.DOFade(0f, FADE_OUT_DURATION));
        }

        private void OnDestroy()
        {
            _activeSequence?.Kill();
        }
    }
}
