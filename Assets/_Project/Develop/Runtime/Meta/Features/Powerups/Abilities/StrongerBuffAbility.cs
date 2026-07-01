using Assets._Project.Develop.Runtime.Configs.Meta.NewPowerups;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.PlantBuildingBuff;
using Assets._Project.Develop.Runtime.Meta.Features.Powerups;

namespace _Project.Develop.Runtime.Meta.Features.Powerups.Abilities
{
    public class StrongerBuffAbility : Powerup
    {
        private readonly PlantBuildingBuffService _plantBuildingBuffService;
        private readonly PermanentStrongerBuffConfig _config;

        public StrongerBuffAbility(
            Entity mainHero,
            PermanentStrongerBuffConfig config,
            int currentLevel,
            PlantBuildingBuffService plantBuildingBuffService) : base(config.ID, currentLevel, config.MaxLevel)
        {
            _config = config;
            _plantBuildingBuffService = plantBuildingBuffService;
        }

        public override void Activate()
        {
            _plantBuildingBuffService.SetStrongerBuffPowerup(
                _config.BuffDamageMultiplier,
                _config.BuffEssenceCostMultiplier);
        }
    }
}
