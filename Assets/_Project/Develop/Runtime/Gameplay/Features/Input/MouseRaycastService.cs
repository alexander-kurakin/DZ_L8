using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.Input
{
    public class MouseRaycastService : IMouseRaycastService
    {
        private readonly Camera _camera;
        
        public MouseRaycastService(Camera camera)
        {
            _camera = camera;
        }
        
        public bool TryGetHit(Vector2 screenPosition, out RaycastHit hit, float maxDistance, int layerMask)
        {
            Ray ray = _camera.ScreenPointToRay(screenPosition);
            return Physics.Raycast(ray, out hit, maxDistance, layerMask);
        }
    }
}