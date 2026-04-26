using System;
using Assets._Project.Develop.Runtime.UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Project.Develop.Runtime.UI.CommonViews
{
    public class IconView : MonoBehaviour, IView
    {
        public event Action IconClicked;
        
        [SerializeField] private Image _icon;
        [SerializeField] private Image _highlightedImage;
        [SerializeField] private RectTransform _rectTransform;

        public RectTransform RectTransform => _rectTransform;
        
        public void OnIconClicked() => IconClicked?.Invoke();
        
        public void SetIcon(Sprite icon) => _icon.sprite = icon;
        
        public void SetHighlighted(bool active) => _highlightedImage.gameObject.SetActive(active);
    }
}