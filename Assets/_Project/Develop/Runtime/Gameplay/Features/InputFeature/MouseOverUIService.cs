using System.Collections.Generic;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.InputFeature
{
    public class MouseOverUIService
    {
        private readonly List<RectTransform> _uiRects = new();

        public void Register(RectTransform rectTransform)
        {
            if (rectTransform != null && !_uiRects.Contains(rectTransform))
                _uiRects.Add(rectTransform);
        }

        public void Unregister(RectTransform rectTransform)
        {
            _uiRects.Remove(rectTransform);
        }

        public bool IsPointerOverUI(Vector2 pointerPosition)
        {
            for (int i = 0; i < _uiRects.Count; i++)
            {
                RectTransform rectTransform = _uiRects[i];
                
                if (rectTransform == null || !rectTransform.gameObject.activeInHierarchy)
                    continue;
                
                if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, pointerPosition, null))
                    return true;
            }
            return false;
        }
    }
}