using UnityEngine;

namespace _Project.Develop.Runtime.UI.Gameplay
{
    public class EntitiesOverlayDisplayView : MonoBehaviour
    {
        [SerializeField] private RectTransform _parent;

        public void Add(RectTransform item)
        {
            item.SetParent(_parent, worldPositionStays: false);
        }

        public void Remove(RectTransform item)
        {
            item.SetParent(null, worldPositionStays: false);
        }

        public void UpdatePositionFor(RectTransform item, Vector3 worldPosition)
        {
            Camera camera = Camera.main;

            if (camera == null)
                return;

            Vector3 screenPoint = camera.WorldToScreenPoint(worldPosition);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_parent, screenPoint, null, out Vector2 localPoint);
            item.anchoredPosition = localPoint;
        }
    }
}
