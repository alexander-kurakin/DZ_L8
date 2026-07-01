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

        public static SectorId ResolveForFlyingEnemy(Vector3 worldPosition, Vector3 center, SectorGridConfig gridConfig)
        {
            SectorId sector = Resolve(worldPosition, center, gridConfig);

            if (sector.Belt == SectorBelt.Spawn || sector.Belt == SectorBelt.Inner)
                return sector;

            return new SectorId(SectorBelt.Inner, sector.Index);
        }

        public static bool IsWorldPositionInSectorWedge(
            Vector3 worldPosition,
            Vector3 center,
            SectorId sectorId,
            SectorGridConfig gridConfig)
        {
            Vector3 offset = worldPosition - center;
            offset.y = 0f;
            float distance = offset.magnitude;

            float innerRadius = GetBeltInnerRadius(sectorId.Belt, gridConfig);
            float outerRadius = GetBeltOuterRadius(sectorId.Belt, gridConfig);

            if (distance > outerRadius)
                return false;

            if (sectorId.Belt != SectorBelt.Inner && distance < innerRadius)
                return false;

            return IsWorldPositionOnPathIndex(worldPosition, center, sectorId.Index);
        }

        public static bool IsWorldPositionOnPathIndex(Vector3 worldPosition, Vector3 center, int pathIndex)
        {
            Vector3 offset = worldPosition - center;
            offset.y = 0f;

            float sectorWidthRadians = FULL_CIRCLE_RADIANS / SectorId.SectorsPerRing;
            float wedgeMinAngle = pathIndex * sectorWidthRadians;
            float wedgeMaxAngle = wedgeMinAngle + sectorWidthRadians;
            float enemyAngle = Mathf.Atan2(offset.z, offset.x);

            if (enemyAngle < 0f)
                enemyAngle += FULL_CIRCLE_RADIANS;

            return IsAngleInWedge(enemyAngle, wedgeMinAngle, wedgeMaxAngle);
        }

        private static bool IsAngleInWedge(float angle, float wedgeMinAngle, float wedgeMaxAngle)
        {
            if (wedgeMinAngle <= wedgeMaxAngle)
                return angle >= wedgeMinAngle && angle < wedgeMaxAngle;

            return angle >= wedgeMinAngle || angle < wedgeMaxAngle;
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

        public static Vector3 GetSectorWedgeMarkerPosition(
            Vector3 center,
            SectorId sectorId,
            SectorGridConfig gridConfig,
            float wedgeAngleFraction,
            float beltRadiusFraction,
            float groundYOffset)
        {
            float sectorWidthRadians = FULL_CIRCLE_RADIANS / SectorId.SectorsPerRing;
            float angleRadians = sectorId.Index * sectorWidthRadians + sectorWidthRadians * wedgeAngleFraction;
            float innerRadius = GetBeltInnerRadius(sectorId.Belt, gridConfig);
            float outerRadius = GetBeltOuterRadius(sectorId.Belt, gridConfig);
            float radius = Mathf.Lerp(innerRadius, outerRadius, beltRadiusFraction);

            return new Vector3(
                center.x + Mathf.Cos(angleRadians) * radius,
                center.y + groundYOffset,
                center.z + Mathf.Sin(angleRadians) * radius);
        }

        public static float GetSectorArcWidth(SectorBelt belt, SectorGridConfig gridConfig)
        {
            float radius = GetAnchorRadius(belt, gridConfig);
            return FULL_CIRCLE_RADIANS * radius / SectorId.SectorsPerRing;
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

        public static float ResolveInboundBeltProgress(
            Vector3 worldPosition,
            Vector3 center,
            SectorBelt belt,
            SectorGridConfig gridConfig)
        {
            Vector3 offset = worldPosition - center;
            offset.y = 0f;
            float distance = offset.magnitude;
            float outerRadius = GetBeltOuterRadius(belt, gridConfig);
            float innerRadius = GetBeltInnerRadius(belt, gridConfig);
            float depth = outerRadius - innerRadius;

            if (depth <= 0f)
                return 0f;

            float progress = (outerRadius - distance) / depth;

            return Mathf.Clamp01(progress);
        }

        public static float GetBeltInnerRadius(SectorBelt belt, SectorGridConfig gridConfig)
        {
            switch (belt)
            {
                case SectorBelt.Inner:
                    return 0f;

                case SectorBelt.Middle:
                    return gridConfig.InnerBeltMaxRadius;

                case SectorBelt.Outer:
                    return gridConfig.MiddleBeltMaxRadius;

                default:
                    return gridConfig.OuterBeltMaxRadius;
            }
        }

        public static float GetBeltOuterRadius(SectorBelt belt, SectorGridConfig gridConfig)
        {
            switch (belt)
            {
                case SectorBelt.Inner:
                    return gridConfig.InnerBeltMaxRadius;

                case SectorBelt.Middle:
                    return gridConfig.MiddleBeltMaxRadius;

                case SectorBelt.Outer:
                    return gridConfig.OuterBeltMaxRadius;

                default:
                    return gridConfig.SpawnBeltMaxRadius;
            }
        }
    }
}
