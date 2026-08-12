using Assets._Project.Develop.Runtime.UI.CommonViews;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.UI.Gameplay.CoachHintArrows
{
    public class CoachHintArrowsDisplay : ElementsListView<CoachHintArrowView>
    {
        private Camera _camera;

        private void Awake()
        {
            _camera = Camera.main;
        }

        public void UpdatePositionFor(CoachHintArrowView arrow, Vector3 worldPosition)
        {
            if (_camera == null)
                _camera = Camera.main;

            if (_camera == null)
                return;

            Vector3 screenPosition = _camera.WorldToScreenPoint(worldPosition);

            if (screenPosition.z < 0f)
            {
                arrow.gameObject.SetActive(false);
                return;
            }

            arrow.gameObject.SetActive(true);
            arrow.transform.position = screenPosition;
        }

        public void UpdateScreenPositionFor(CoachHintArrowView arrow, Vector3 screenPosition)
        {
            arrow.gameObject.SetActive(true);
            arrow.transform.position = screenPosition;
        }
    }
}
