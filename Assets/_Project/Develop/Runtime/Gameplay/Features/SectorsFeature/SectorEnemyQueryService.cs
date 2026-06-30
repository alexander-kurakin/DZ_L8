using System.Collections.Generic;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
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

        private bool IsEnemyInSector(Entity entity, SectorId sectorId)
        {
            if (IsEnemyAlive(entity) == false)
                return false;

            SectorId enemySector = ResolveEnemySector(entity);
            return enemySector == sectorId;
        }

        private SectorId ResolveEnemySector(Entity entity)
        {
            if (entity.TryGetTransform(out Transform transform))
                return _sectorMembershipService.ResolveFromWorldPosition(transform.position);

            if (entity.TryGetCurrentSector(out ReactiveVariable<SectorId> currentSector))
                return currentSector.Value;

            return default;
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
