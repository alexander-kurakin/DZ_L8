using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.PlantBuildingBuff
{
    public class BuildingBuffSystem : IUpdatableSystem
    {
        private readonly PlantBuildingBuffService _plantBuildingBuffService;

        public BuildingBuffSystem(PlantBuildingBuffService plantBuildingBuffService)
        {
            _plantBuildingBuffService = plantBuildingBuffService;
        }

        public void OnUpdate(float deltaTime)
        {
            _plantBuildingBuffService.Tick(deltaTime);
        }
    }
}
