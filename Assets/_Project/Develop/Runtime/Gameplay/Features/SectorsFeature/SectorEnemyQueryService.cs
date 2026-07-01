using System.Collections.Generic;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Sectors;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.PlantCombatFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.TeamsFeature;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature
{
    public class SectorEnemyQueryService
    {
        private readonly EntitiesLifeContext _entitiesLifeContext;
        private readonly SectorMembershipService _sectorMembershipService;

        public SectorEnemyQueryService(
            EntitiesLifeContext entitiesLifeContext,
            SectorMembershipService sectorMembershipService)
        {
            _entitiesLifeContext = entitiesLifeContext;
            _sectorMembershipService = sectorMembershipService;
        }

        public void CollectEnemiesInSector(SectorId sectorId, List<Entity> results)
        {
            results.Clear();

            foreach (Entity entity in _entitiesLifeContext.Entities)
            {
                if (IsEnemyInSector(entity, sectorId) == false)
                    continue;

                results.Add(entity);
            }
        }

        public void CollectEnemiesInMineSector(
            SectorId sectorId,
            Vector3 sectorCenter,
            Vector3 mineWorldPosition,
            SectorGridConfig gridConfig,
            bool includeMineCellProximity,
            List<Entity> results)
        {
            results.Clear();

            float wedgeHalfArc = WorldToSector.GetSectorArcWidth(sectorId.Belt, gridConfig) * 0.5f;
            float mineCellRadiusSqr = wedgeHalfArc * wedgeHalfArc;

            foreach (Entity entity in _entitiesLifeContext.Entities)
            {
                if (IsEnemyAlive(entity) == false)
                    continue;

                if (entity.TryGetTransform(out Transform transform) == false)
                    continue;

                Vector3 enemyPosition = transform.position;
                bool inWedge = WorldToSector.IsWorldPositionInSectorWedge(
                    enemyPosition,
                    sectorCenter,
                    sectorId,
                    gridConfig);

                if (inWedge)
                {
                    results.Add(entity);
                    continue;
                }

                if (includeMineCellProximity == false)
                    continue;

                Vector3 offsetFromMine = enemyPosition - mineWorldPosition;
                offsetFromMine.y = 0f;

                if (offsetFromMine.sqrMagnitude > mineCellRadiusSqr)
                    continue;

                if (WorldToSector.IsWorldPositionOnPathIndex(enemyPosition, sectorCenter, sectorId.Index) == false)
                    continue;

                results.Add(entity);
            }
        }

        public void AppendEnemiesInSector(SectorId sectorId, List<Entity> results)
        {
            foreach (Entity entity in _entitiesLifeContext.Entities)
            {
                if (IsEnemyInSector(entity, sectorId) == false)
                    continue;

                if (results.Contains(entity))
                    continue;

                results.Add(entity);
            }
        }

        public void AppendEnemiesNearWorldPosition(Vector3 worldPosition, float radius, List<Entity> results)
        {
            float radiusSqr = radius * radius;

            foreach (Entity entity in _entitiesLifeContext.Entities)
            {
                if (IsEnemyAlive(entity) == false)
                    continue;

                if (entity.TryGetTransform(out Transform transform) == false)
                    continue;

                Vector3 offset = transform.position - worldPosition;
                offset.y = 0f;

                if (offset.sqrMagnitude > radiusSqr)
                    continue;

                if (results.Contains(entity))
                    continue;

                results.Add(entity);
            }
        }

        public void CollectEnemiesOnBelt(SectorBelt belt, List<Entity> results)
        {
            results.Clear();

            foreach (Entity entity in _entitiesLifeContext.Entities)
            {
                if (IsEnemyAlive(entity) == false)
                    continue;

                SectorId enemySector = ResolveEnemySector(entity);

                if (enemySector.Belt != belt)
                    continue;

                results.Add(entity);
            }
        }

        public void CollectEnemiesOnBelts(SectorBelt firstBelt, SectorBelt secondBelt, List<Entity> results)
        {
            results.Clear();

            foreach (Entity entity in _entitiesLifeContext.Entities)
            {
                if (IsEnemyAlive(entity) == false)
                    continue;

                SectorId enemySector = ResolveEnemySector(entity);

                if (enemySector.Belt != firstBelt && enemySector.Belt != secondBelt)
                    continue;

                results.Add(entity);
            }
        }

        public void CollectEnemiesOnPath(int pathIndex, List<Entity> results)
        {
            results.Clear();

            foreach (Entity entity in _entitiesLifeContext.Entities)
            {
                if (IsEnemyAlive(entity) == false)
                    continue;

                SectorId enemySector = ResolveEnemySector(entity);

                if (enemySector.Index != pathIndex)
                    continue;

                results.Add(entity);
            }
        }

        public void CollectEnemiesOnPlantablePath(int pathIndex, List<Entity> results)
        {
            results.Clear();

            foreach (Entity entity in _entitiesLifeContext.Entities)
            {
                if (IsEnemyAlive(entity) == false)
                    continue;

                SectorId enemySector = ResolveEnemySector(entity);

                if (enemySector.Index != pathIndex)
                    continue;

                if (IsPlantableBelt(enemySector.Belt) == false)
                    continue;

                results.Add(entity);
            }
        }

        public void CollectEnemiesOnBeltAtPathIndices(SectorBelt belt, int centerPathIndex, List<Entity> results)
        {
            results.Clear();

            int indexMinus = WrapPathIndex(centerPathIndex - 1);
            int indexCenter = WrapPathIndex(centerPathIndex);
            int indexPlus = WrapPathIndex(centerPathIndex + 1);

            foreach (Entity entity in _entitiesLifeContext.Entities)
            {
                if (IsEnemyAlive(entity) == false)
                    continue;

                SectorId enemySector = ResolveEnemySector(entity);

                if (enemySector.Belt != belt)
                    continue;

                if (enemySector.Index != indexMinus
                    && enemySector.Index != indexCenter
                    && enemySector.Index != indexPlus)
                {
                    continue;
                }

                results.Add(entity);
            }
        }

        public static int WrapPathIndex(int pathIndex)
        {
            int wrappedIndex = pathIndex % SectorId.SectorsPerRing;

            if (wrappedIndex < 0)
                wrappedIndex += SectorId.SectorsPerRing;

            return wrappedIndex;
        }

        private static bool IsPlantableBelt(SectorBelt belt)
        {
            return belt == SectorBelt.Inner
                   || belt == SectorBelt.Middle
                   || belt == SectorBelt.Outer;
        }

        private bool IsEnemyInSector(Entity entity, SectorId sectorId)
        {
            if (IsEnemyAlive(entity) == false)
                return false;

            SectorId enemySector = ResolveEnemySector(entity);
            return enemySector == sectorId;
        }

        private SectorId ResolveEnemySector(Entity entity)
        {
            if (entity.TryGetCurrentSector(out ReactiveVariable<SectorId> currentSector))
                return currentSector.Value;

            if (entity.TryGetTransform(out Transform transform) == false)
                return default;

            if (entity.HasComponent<FlyingEnemy>())
                return _sectorMembershipService.ResolveFlyingEnemyFromWorldPosition(transform.position);

            return _sectorMembershipService.ResolveFromWorldPosition(transform.position);
        }

        private static bool IsEnemyAlive(Entity entity)
        {
            if (entity.TryGetTeam(out ReactiveVariable<Teams> team) == false)
                return false;

            if (team.Value != Teams.Enemies)
                return false;

            if (entity.TryGetCurrentHealth(out ReactiveVariable<float> health) && health.Value <= 0f)
                return false;

            return true;
        }
    }
}
