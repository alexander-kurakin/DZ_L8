using UnityEngine;
using Vector2 = UnityEngine.Vector2;

namespace _Project.Develop.Runtime.Gameplay.Features.Input
{
    public interface IMouseRaycastService
    {
        Camera Camera { get; }

        bool TryGetHit(Vector2 screenPosition, out RaycastHit hit, float maxDistance, int layerMask);

        bool TryGetHorizontalPlaneHit(Vector2 screenPosition, float planeY, out Vector3 hitPoint);
    }
}