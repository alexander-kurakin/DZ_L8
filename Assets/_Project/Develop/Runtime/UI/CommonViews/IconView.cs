using Assets._Project.Develop.Runtime.UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Project.Develop.Runtime.UI.CommonViews
{
    public class IconView : MonoBehaviour, IView
    {
        [SerializeField] private Image _icon;
        [SerializeField] private Image _highlightedImage;

        public void SetIcon(Sprite icon) => _icon.sprite = icon;
        
        public void SetHighlightedImage(Sprite icon) => _highlightedImage.sprite = icon;
        
        public void SetActive(bool active) => _highlightedImage.gameObject.SetActive(active);
    }
}