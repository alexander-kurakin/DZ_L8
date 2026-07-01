using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Utilities.Reactive;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature
{
    public class CurrentSector : IEntityComponent
    {
        public ReactiveVariable<SectorId> Value;
    }

    public class PlantSector : IEntityComponent
    {
        public SectorId Value;
    }

    public class PlantedEssenceCost : IEntityComponent
    {
        public int Value;
    }
}
