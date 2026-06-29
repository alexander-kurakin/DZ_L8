using _Project.Develop.Runtime.Gameplay.Features.Input;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature
{
    public static class SectorSurfaceClickUtility
    {
        public static bool TryGetHorizontalPlanePoint(
            IMouseRaycastService raycastService,
            Vector2 screenPosition,
            SectorRegistryService sectorRegistry,
            out Vector3 hitPoint)
        {
            hitPoint = default;

            if (sectorRegistry.IsInitialized == false)
                return false;

            float planeY = sectorRegistry.Center.y + sectorRegistry.GridConfig.SectorSurfaceGroundYOffset;

            if (raycastService.TryGetHorizontalPlaneHit(screenPosition, planeY, out Vector3 planePoint) == false)
                return false;

            hitPoint = planePoint;
            return true;
        }

        public static bool TryGetArenaPlanePoint(
            IMouseRaycastService raycastService,
            Vector2 screenPosition,
            SectorRegistryService sectorRegistry,
            out Vector3 hitPoint)
        {
            if (TryGetHorizontalPlanePoint(raycastService, screenPosition, sectorRegistry, out hitPoint) == false)
                return false;

            Vector3 offsetFromCenter = hitPoint - sectorRegistry.Center;
            offsetFromCenter.y = 0f;
            float maxRadius = sectorRegistry.GridConfig.SpawnBeltMaxRadius + sectorRegistry.GridConfig.ClickArenaRadiusMargin;

            if (offsetFromCenter.sqrMagnitude > maxRadius * maxRadius)
                return false;

            return true;
        }
    }
}
