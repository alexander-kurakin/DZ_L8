using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature
{
    public class MouseRaycastService : IMouseRaycastService
    {
        private readonly Camera _camera;
        
        public MouseRaycastService(Camera camera)
        {
            _camera = camera;
        }

        public Camera Camera => _camera;
        
        public bool TryGetHit(Vector2 screenPosition, out RaycastHit hit, float maxDistance, int layerMask)
        {
            Ray ray = _camera.ScreenPointToRay(screenPosition);
            return Physics.Raycast(ray, out hit, maxDistance, layerMask);
        }

        public bool TryGetHorizontalPlaneHit(Vector2 screenPosition, float planeY, out Vector3 hitPoint)
        {
            hitPoint = default;

            if (_camera == null)
                return false;

            Ray ray = _camera.ScreenPointToRay(screenPosition);
            Plane plane = new Plane(Vector3.up, new Vector3(0f, planeY, 0f));

            if (plane.Raycast(ray, out float distance) == false)
                return false;

            hitPoint = ray.GetPoint(distance);
            return true;
        }
    }
}