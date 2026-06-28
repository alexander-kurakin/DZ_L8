using System.Collections.Generic;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.TeamsFeature;
using Assets._Project.Develop.Runtime.Utilities.Reactive;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature
{
    public class SectorEnemyQueryService
    {
        private readonly EntitiesLifeContext _entitiesLifeContext;

        public SectorEnemyQueryService(EntitiesLifeContext entitiesLifeContext)
        {
            _entitiesLifeContext = entitiesLifeContext;
        }

        public void CollectEnemiesInSector(SectorId sectorId, List<Entity> results)
        {
            results.Clear();

            foreach (Entity entity in _entitiesLifeContext.Entities)
            {
                if (entity.TryGetCurrentSector(out ReactiveVariable<SectorId> currentSector) == false)
                    continue;

                if (currentSector.Value != sectorId)
                    continue;

                if (entity.TryGetTeam(out ReactiveVariable<Teams> team) == false)
                    continue;

                if (team.Value != Teams.Enemies)
                    continue;

                if (entity.TryGetCurrentHealth(out ReactiveVariable<float> health) && health.Value <= 0f)
                    continue;

                results.Add(entity);
            }
        }
    }
}
