using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature;
using Assets._Project.Develop.Runtime.Utilities.Reactive;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.PlantCombatFeature
{
    public class TurretCombatTargetRefreshSystem : IInitializableSystem, IUpdatableSystem
    {
        private readonly SectorEnemyQueryService _sectorEnemyQueryService;

        private TurretPathTargetSelector _selector;
        private ReactiveVariable<Entity> _currentTarget;

        public TurretCombatTargetRefreshSystem(SectorEnemyQueryService sectorEnemyQueryService)
        {
            _sectorEnemyQueryService = sectorEnemyQueryService;
        }

        public void OnInit(Entity entity)
        {
            _selector = new TurretPathTargetSelector(entity, _sectorEnemyQueryService);
            _currentTarget = entity.CurrentTarget;
        }

        public void OnUpdate(float deltaTime)
        {
            _currentTarget.Value = _selector.SelectTargetFrom(null);
        }
    }
}
