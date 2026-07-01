using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Utilities.Reactive;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.PlantBuildingBuff
{
    public class BuildingBuffRemainingSeconds : IEntityComponent
    {
        public ReactiveVariable<float> Value;
    }
}
