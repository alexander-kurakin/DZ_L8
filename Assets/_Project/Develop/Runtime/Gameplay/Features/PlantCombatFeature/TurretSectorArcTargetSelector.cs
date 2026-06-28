using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.AI.States;
using Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.TakeDamage;
using Assets._Project.Develop.Runtime.Gameplay.Features.TeamsFeature;
using Assets._Project.Develop.Runtime.Utilities.Conditions;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System.Collections.Generic;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.PlantCombatFeature
{
    public class TurretSectorArcTargetSelector : ITargetSelector
    {
        private readonly Entity _turretEntity;
        private readonly Transform _turretTransform;
        private readonly SectorEnemyQueryService _sectorEnemyQueryService;
        private readonly List<Entity> _candidates = new();

        public TurretSectorArcTargetSelector(
            Entity turretEntity,
            SectorEnemyQueryService sectorEnemyQueryService)
        {
            _turretEntity = turretEntity;
            _turretTransform = turretEntity.Transform;
            _sectorEnemyQueryService = sectorEnemyQueryService;
        }

        public Entity SelectTargetFrom(IEnumerable<Entity> targets)
        {
            if (_turretEntity.TryGetPlantSector(out SectorId sectorId) == false)
                return null;

            _sectorEnemyQueryService.CollectEnemiesOnBeltAtPathIndices(sectorId.Belt, sectorId.Index, _candidates);

            Entity closestTarget = null;
            float minDistance = float.MaxValue;

            for (int index = 0; index < _candidates.Count; index++)
            {
                Entity candidate = _candidates[index];

                if (IsValidTarget(candidate) == false)
                    continue;

                float distance = (_turretTransform.position - candidate.Transform.position).sqrMagnitude;

                if (distance >= minDistance)
                    continue;

                minDistance = distance;
                closestTarget = candidate;
            }

            return closestTarget;
        }

        private bool IsValidTarget(Entity target)
        {
            if (target.HasComponent<TakeDamageRequest>() == false)
                return false;

            if (target.TryGetCanTakeDamage(out ICompositeCondition canApplyDamage))
            {
                if (canApplyDamage.Evaluate() == false)
                    return false;
            }

            if (_turretEntity.TryGetTeam(out ReactiveVariable<Teams> sourceTeam)
                && target.TryGetTeam(out ReactiveVariable<Teams> targetTeam))
            {
                if (sourceTeam.Value == targetTeam.Value)
                    return false;
            }

            if (target == _turretEntity)
                return false;

            return true;
        }
    }
}
