using Assets._Project.Develop.Runtime.UI.CommonViews;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.UI.Gameplay.PlantBuildingBuff
{
    public class PlantBuildingBuffTimersDisplay : ElementsListView<TextView>
    {
        private Camera _camera;

        private void Awake()
        {
            _camera = Camera.main;
        }

        public void UpdatePositionFor(TextView timerView, Vector3 worldPosition)
        {
            if (_camera == null)
                _camera = Camera.main;

            if (_camera == null)
                return;

            Vector3 screenPosition = _camera.WorldToScreenPoint(worldPosition);
            timerView.transform.position = screenPosition;
        }
    }
}
