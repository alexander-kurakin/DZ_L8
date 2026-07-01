using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Develop.Runtime.UI.Gameplay.LmbFlavorToast
{
    public class LmbFlavorToastView : MonoBehaviour
    {
        private const float FADE_IN_DURATION_SECONDS = 0.22f;
        private const float VISIBLE_DURATION_SECONDS = 2.4f;
        private const float FADE_OUT_DURATION_SECONDS = 0.32f;
        private const float SLIDE_OFFSET_Y = 28f;
        private const float BACKGROUND_ALPHA = 0.72f;

        private CanvasGroup _canvasGroup;
        private RectTransform _rootRectTransform;
        private TextMeshProUGUI _label;
        private Vector2 _restAnchoredPosition;
        private Sequence _activeSequence;

        public void Initialize()
        {
            _rootRectTransform = gameObject.AddComponent<RectTransform>();
            _rootRectTransform.anchorMin = new Vector2(0.5f, 0.22f);
            _rootRectTransform.anchorMax = new Vector2(0.5f, 0.22f);
            _rootRectTransform.pivot = new Vector2(0.5f, 0.5f);
            _rootRectTransform.sizeDelta = new Vector2(920f, 96f);
            _restAnchoredPosition = _rootRectTransform.anchoredPosition;

            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;

            CreateBackground();
            CreateLabel();
        }

        public void Show(string message)
        {
            _label.text = message;

            _activeSequence?.Kill();

            _canvasGroup.alpha = 0f;
            _rootRectTransform.anchoredPosition =
                _restAnchoredPosition + new Vector2(0f, -SLIDE_OFFSET_Y);

            _activeSequence = DOTween.Sequence()
                .Append(_rootRectTransform
                    .DOAnchorPos(_restAnchoredPosition, FADE_IN_DURATION_SECONDS)
                    .SetEase(Ease.OutQuad))
                .Join(_canvasGroup.DOFade(1f, FADE_IN_DURATION_SECONDS))
                .AppendInterval(VISIBLE_DURATION_SECONDS)
                .Append(_canvasGroup.DOFade(0f, FADE_OUT_DURATION_SECONDS))
                .Join(_rootRectTransform
                    .DOAnchorPos(
                        _restAnchoredPosition + new Vector2(0f, SLIDE_OFFSET_Y * 0.35f),
                        FADE_OUT_DURATION_SECONDS)
                    .SetEase(Ease.InQuad))
                .Play();
        }

        private void CreateBackground()
        {
            GameObject backgroundObject = new GameObject("Background");
            backgroundObject.transform.SetParent(transform, false);

            RectTransform backgroundRect = backgroundObject.AddComponent<RectTransform>();
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.offsetMin = Vector2.zero;
            backgroundRect.offsetMax = Vector2.zero;

            Image backgroundImage = backgroundObject.AddComponent<Image>();
            backgroundImage.color = new Color(0f, 0f, 0f, BACKGROUND_ALPHA);
            backgroundImage.raycastTarget = false;
        }

        private void CreateLabel()
        {
            GameObject labelObject = new GameObject("Label");
            labelObject.transform.SetParent(transform, false);

            RectTransform labelRect = labelObject.AddComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(24f, 12f);
            labelRect.offsetMax = new Vector2(-24f, -12f);

            _label = labelObject.AddComponent<TextMeshProUGUI>();
            _label.font = TMP_Settings.defaultFontAsset;
            _label.alignment = TextAlignmentOptions.Center;
            _label.fontSize = 30f;
            _label.color = Color.white;
            _label.enableWordWrapping = true;
            _label.raycastTarget = false;
            _label.outlineWidth = 0.18f;
            _label.outlineColor = new Color32(0, 0, 0, 200);
        }

        private void OnDestroy()
        {
            _activeSequence?.Kill();
        }
    }
}
