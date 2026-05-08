using System;
using Assets._Project.Develop.Runtime.UI.Core;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Project.Develop.Runtime.UI.MainMenu.ShopPopup
{
  public class SelectableAbilityView : MonoBehaviour, IShowableView
    {
        public event Action Clicked;

        [SerializeField] private CanvasGroup _canvasGroup;

        [SerializeField] private Button _button;

        [SerializeField] private AbilityIcon _icon;
        [SerializeField] private TMP_Text _name;
        [SerializeField] private TMP_Text _description;

        [SerializeField] private TMP_Text _textOnTablet;

        [SerializeField] private Image _selectImage;
        [SerializeField] private Image _tabletImage;

        private Sequence _currentAnimation;
        private float _startYOffset = 100;

        public AbilityIcon Icon => _icon;

        private void Awake()
        {
            _canvasGroup.alpha = 0;
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(OnClicked);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnClicked);
        }

        public void SetName(string name) => _name.text = name;

        public void SetDescription(string description) => _description.text = description;

        public void SetTabletText(string tabletText) => _textOnTablet.text = tabletText;
        public void SetTabletColor(Color color) => _tabletImage.color = color;

        public void Select() => _selectImage.gameObject.SetActive(true);

        public void Deselect() => _selectImage.gameObject.SetActive(false);

        private void OnClicked() => Clicked?.Invoke();


        public Tween Hide()
        {
            _currentAnimation?.Kill();

            _currentAnimation = DOTween.Sequence();

            return _currentAnimation
                .Append(_canvasGroup.DOFade(0, 0.1f))
                .Join(_canvasGroup.transform.DOLocalMoveY(_startYOffset, 0.1f))
                .SetUpdate(true)
                .Play();
        }

        public Tween Show()
        {
            _currentAnimation?.Kill();

            _currentAnimation = DOTween.Sequence();

            return _currentAnimation
                .Append(_canvasGroup.DOFade(1, 0.2f))
                .Join(_canvasGroup.transform.DOLocalMoveY(0, 0.2f).From(_startYOffset))
                .SetUpdate(true)
                .Play();
        }
        
        public Tween CannotBuy()
        {
            _currentAnimation?.Kill();
            _currentAnimation = DOTween.Sequence();
            
            const float fadeInDuration = 0.18f;
            const float wobbleStepDuration = 0.11f;
            const float settleDuration = 0.14f;
            float swingAmplitude = 16f;
            
            Transform contentTransform = _canvasGroup.transform;
            Vector3 homeLocalPosition = contentTransform.localPosition;

            return _currentAnimation
                .Append(_canvasGroup.DOFade(1f, fadeInDuration))
                .Append(contentTransform.DOLocalMove(
                    homeLocalPosition + new Vector3(-swingAmplitude, 0f, 0f), wobbleStepDuration).SetEase(Ease.OutQuad))
                .Append(contentTransform.DOLocalMove(
                    homeLocalPosition + new Vector3(swingAmplitude * 0.65f, 0f, 0f), wobbleStepDuration).SetEase(Ease.OutQuad))
                .Append(contentTransform.DOLocalMove(
                    homeLocalPosition + new Vector3(-swingAmplitude * 0.35f, 0f, 0f), wobbleStepDuration).SetEase(Ease.OutQuad))
                .Append(contentTransform.DOLocalMove(
                    homeLocalPosition + new Vector3(swingAmplitude * 0.15f, 0f, 0f), wobbleStepDuration).SetEase(Ease.OutQuad))
                .Append(contentTransform.DOLocalMove(homeLocalPosition, settleDuration).SetEase(Ease.OutElastic, 0.9f, 0.45f))
                .SetUpdate(true)
                .Play();
        }

        public Tween SuccessfulBuy()
        {
            _currentAnimation?.Kill();
            _currentAnimation = DOTween.Sequence();
            
            const float fadeInDuration = 0.18f;
            float hopHeight = 18f;
            
            Transform contentTransform = _canvasGroup.transform;
            Vector3 startLocal = contentTransform.localPosition;
            Vector3 landedLocal = new Vector3(startLocal.x, 0f, startLocal.z);
            
            return _currentAnimation
                .Append(_canvasGroup.DOFade(1f, fadeInDuration))
                .Join(contentTransform.DOLocalMoveY(0f, fadeInDuration).From(_startYOffset))
                .Append(contentTransform.DOLocalMove(landedLocal + new Vector3(0f, hopHeight, 0f), 0.11f).SetEase(Ease.OutQuad))
                .Append(contentTransform.DOLocalMove(landedLocal, 0.09f).SetEase(Ease.InQuad))
                .Append(contentTransform.DOLocalMove(landedLocal + new Vector3(0f, hopHeight * 0.55f, 0f), 0.1f).SetEase(Ease.OutQuad))
                .Append(contentTransform.DOLocalMove(landedLocal, 0.09f).SetEase(Ease.InQuad))
                .Append(contentTransform.DOLocalMove(landedLocal + new Vector3(0f, hopHeight * 0.22f, 0f), 0.09f).SetEase(Ease.OutQuad))
                .Append(contentTransform.DOLocalMove(landedLocal, 0.12f).SetEase(Ease.OutQuad))
                .SetUpdate(true)
                .Play();
        }

        private void OnDestroy()
        {
            _currentAnimation?.Kill();
        }
    }
}