using Assets._Project.Develop.Runtime.UI.Core;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Project.Develop.Runtime.UI.Gameplay.Stages
{
    public class WavePreviewView : MonoBehaviour, IView
    {
        private const float ICON_SIZE = 80f;

        [SerializeField] private RectTransform _iconsLayoutRoot;
        [SerializeField] private Material _iconOutlineMaterial;

        private readonly List<Image> _iconImages = new();

        private void Awake()
        {
            if (_iconsLayoutRoot == null)
                _iconsLayoutRoot = transform as RectTransform;
        }

        public void SetIcons(IReadOnlyList<Sprite> icons)
        {
            ClearIcons();

            if (icons == null || icons.Count == 0)
                return;

            for (int iconIndex = 0; iconIndex < icons.Count; iconIndex++)
            {
                Image iconImage = CreateIconImage(iconIndex, icons[iconIndex]);
                _iconImages.Add(iconImage);
            }
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        public void ClearIcons()
        {
            foreach (Image iconImage in _iconImages)
            {
                if (iconImage != null)
                    Destroy(iconImage.gameObject);
            }

            _iconImages.Clear();
        }

        private Image CreateIconImage(int iconIndex, Sprite sprite)
        {
            GameObject iconObject = new GameObject(
                $"Icon_{iconIndex}",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));

            RectTransform iconRectTransform = iconObject.GetComponent<RectTransform>();
            iconRectTransform.SetParent(_iconsLayoutRoot, false);
            iconRectTransform.sizeDelta = new Vector2(ICON_SIZE, ICON_SIZE);

            Image iconImage = iconObject.GetComponent<Image>();
            iconImage.sprite = sprite;
            iconImage.preserveAspect = true;
            iconImage.raycastTarget = false;

            if (_iconOutlineMaterial != null)
                iconImage.material = _iconOutlineMaterial;

            return iconImage;
        }
    }
}
