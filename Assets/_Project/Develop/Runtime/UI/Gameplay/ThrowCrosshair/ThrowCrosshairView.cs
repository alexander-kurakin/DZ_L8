using UnityEngine;

namespace _Project.Develop.Runtime.UI.Gameplay.ThrowCrosshair
{
    public class ThrowCrosshairView : MonoBehaviour
    {
        [SerializeField] private RectTransform _rectTransform;

        public bool IsVisible { get; private set; }

        public void SetVisible(bool isVisible)
        {
            IsVisible = isVisible;
            gameObject.SetActive(isVisible);
        }

        public void SetScreenPosition(Vector2 screenPosition)
        {
            _rectTransform.position = screenPosition;
        }
    }
}
