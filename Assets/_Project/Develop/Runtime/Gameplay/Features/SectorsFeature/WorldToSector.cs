using Assets._Project.Develop.Runtime.Configs.Gameplay.Sectors;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature
{
    public static class WorldToSector
    {
        private const float FULL_CIRCLE_RADIANS = Mathf.PI * 2f;

        public static SectorId Resolve(Vector3 worldPosition, Vector3 center, SectorGridConfig gridConfig)
        {
            Vector3 offset = worldPosition - center;
            offset.y = 0f;

            int index = ResolveIndex(offset);
            SectorBelt belt = ResolveBelt(offset.magnitude, gridConfig);

            return new SectorId(belt, index);
        }

        public static int ResolveIndex(Vector3 flatOffsetFromCenter)
        {
            float angleRadians = Mathf.Atan2(flatOffsetFromCenter.z, flatOffsetFromCenter.x);

            if (angleRadians < 0f)
                angleRadians += FULL_CIRCLE_RADIANS;

            float sectorWidthRadians = FULL_CIRCLE_RADIANS / SectorId.SectorsPerRing;
            int index = Mathf.FloorToInt(angleRadians / sectorWidthRadians);

            if (index >= SectorId.SectorsPerRing)
                index = SectorId.SectorsPerRing - 1;

            return index;
        }

        public static SectorBelt ResolveBelt(float distanceFromCenter, SectorGridConfig gridConfig)
        {
            if (distanceFromCenter <= gridConfig.InnerBeltMaxRadius)
                return SectorBelt.Inner;

            if (distanceFromCenter <= gridConfig.MiddleBeltMaxRadius)
                return SectorBelt.Middle;

            if (distanceFromCenter <= gridConfig.OuterBeltMaxRadius)
                return SectorBelt.Outer;

            return SectorBelt.Spawn;
        }

        public static Vector3 GetAnchorPosition(Vector3 center, SectorId sectorId, SectorGridConfig gridConfig)
        {
            float radius = GetAnchorRadius(sectorId.Belt, gridConfig);
            float sectorWidthRadians = FULL_CIRCLE_RADIANS / SectorId.SectorsPerRing;
            float angleRadians = sectorId.Index * sectorWidthRadians + sectorWidthRadians * 0.5f;

            float offsetX = Mathf.Cos(angleRadians) * radius;
            float offsetZ = Mathf.Sin(angleRadians) * radius;

            return new Vector3(center.x + offsetX, center.y, center.z + offsetZ);
        }

        public static Vector3 GetPositionInWedge(
            Vector3 center,
            int pathIndex,
            SectorGridConfig gridConfig,
            float angleOffsetRadians,
            float radiusScale)
        {
            float minRadius = gridConfig.OuterBeltMaxRadius;
            float maxRadius = gridConfig.SpawnBeltMaxRadius;
            float radius = Mathf.Lerp(minRadius, maxRadius, radiusScale);

            float sectorWidthRadians = FULL_CIRCLE_RADIANS / SectorId.SectorsPerRing;
            float angleRadians = pathIndex * sectorWidthRadians + angleOffsetRadians;

            float offsetX = Mathf.Cos(angleRadians) * radius;
            float offsetZ = Mathf.Sin(angleRadians) * radius;

            return new Vector3(center.x + offsetX, center.y, center.z + offsetZ);
        }

        private static float GetAnchorRadius(SectorBelt belt, SectorGridConfig gridConfig)
        {
            switch (belt)
            {
                case SectorBelt.Inner:
                    return gridConfig.InnerBeltAnchorRadius;

                case SectorBelt.Middle:
                    return gridConfig.MiddleBeltAnchorRadius;

                case SectorBelt.Outer:
                    return gridConfig.OuterBeltAnchorRadius;

                case SectorBelt.Spawn:
                    return gridConfig.SpawnBeltAnchorRadius;

                default:
                    return gridConfig.SpawnBeltAnchorRadius;
            }
        }
    }
}
