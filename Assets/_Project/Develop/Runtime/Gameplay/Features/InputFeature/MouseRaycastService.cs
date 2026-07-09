using Assets._Project.Develop.Runtime.Configs.Gameplay.MouseConfig;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature
{
    public class MouseRaycastService : IMouseRaycastService
    {
        private readonly Camera _camera;
        private readonly float _maxRaycastDistance;

        public MouseRaycastService(Camera camera, RaycastConfig raycastConfig)
        {
            _camera = camera;
            _maxRaycastDistance = raycastConfig.MouseRaycastDistance;
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

        public bool TryGetThrowAimPointFromScreen(Vector2 screenPosition, Transform ignoreTransform, out Vector3 aimPoint)
        {
            aimPoint = default;

            if (_camera == null)
                return false;

            Ray ray = _camera.ScreenPointToRay(screenPosition);
            RaycastHit[] hits = Physics.RaycastAll(ray, _maxRaycastDistance);

            float closestDistance = float.MaxValue;
            bool hasHit = false;

            foreach (RaycastHit hit in hits)
            {
                if (ignoreTransform != null && hit.transform.IsChildOf(ignoreTransform))
                    continue;

                if (hit.distance >= closestDistance)
                    continue;

                closestDistance = hit.distance;
                aimPoint = hit.point;
                hasHit = true;
            }

            return hasHit;
        }

        public bool TryGetThrowDirectionFromCamera(Vector2 screenPosition, out Vector3 direction)
        {
            direction = default;

            if (_camera == null)
                return false;

            Ray ray = _camera.ScreenPointToRay(screenPosition);
            direction = ray.direction.normalized;
            return true;
        }

        public bool TryGetThrowDirectionFromScreen(
            Vector2 screenPosition,
            Vector3 origin,
            Transform ignoreTransform,
            out Vector3 direction)
        {
            direction = default;

            if (_camera == null)
                return false;

            Ray ray = _camera.ScreenPointToRay(screenPosition);

            if (TryGetThrowAimPointFromScreen(screenPosition, ignoreTransform, out Vector3 aimPoint))
            {
                direction = aimPoint - origin;

                if (direction.sqrMagnitude > 0f)
                {
                    direction.Normalize();
                    return true;
                }
            }

            direction = ray.direction;
            return true;
        }
    }
}