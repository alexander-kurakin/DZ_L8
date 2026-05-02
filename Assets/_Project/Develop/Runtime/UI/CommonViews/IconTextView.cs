using Assets._Project.Develop.Runtime.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Project.Develop.Runtime.UI.CommonViews
{
    public class IconTextView : MonoBehaviour, IView
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private Image _icon;
        [SerializeField] private RectTransform _rectTransform;
        public RectTransform RectTransform => _rectTransform;

        public void SetText(string text) => _text.text = text;

        public void SetIcon(Sprite icon) => _icon.sprite = icon;
    }
}
