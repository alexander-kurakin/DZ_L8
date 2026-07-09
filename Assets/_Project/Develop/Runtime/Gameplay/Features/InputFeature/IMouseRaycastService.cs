using UnityEngine;
using Vector2 = UnityEngine.Vector2;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature
{
    public interface IMouseRaycastService
    {
        Camera Camera { get; }

        bool TryGetHit(Vector2 screenPosition, out RaycastHit hit, float maxDistance, int layerMask);

        bool TryGetHorizontalPlaneHit(Vector2 screenPosition, float planeY, out Vector3 hitPoint);

        bool TryGetThrowAimPointFromScreen(Vector2 screenPosition, Transform ignoreTransform, out Vector3 aimPoint);

        bool TryGetThrowDirectionFromCamera(Vector2 screenPosition, out Vector3 direction);

        bool TryGetThrowDirectionFromScreen(
            Vector2 screenPosition,
            Vector3 origin,
            Transform ignoreTransform,
            out Vector3 direction);
    }
}