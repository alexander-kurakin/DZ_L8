using System;
using _Project.Develop.Runtime.Meta.Features.Powerups.Abilities;
using Assets._Project.Develop.Runtime.Configs.Meta.NewPowerups;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature;
using Assets._Project.Develop.Runtime.Infrastructure.DI;
using Assets._Project.Develop.Runtime.Meta.Features.Powerups;

namespace _Project.Develop.Runtime.Meta.Features.Powerups
{
    public class PowerupFactory
    {
        private DIContainer _container;
        private readonly EntitiesLifeContext _entitiesLifeContext;
        private readonly StageProviderService _stageProviderService;
        
        public PowerupFactory(DIContainer container)
        {
            _container = container;
            _entitiesLifeContext = _container.Resolve<EntitiesLifeContext>();
            _stageProviderService = _container.Resolve<StageProviderService>();
        }

        public Powerup CreatePowerupFor(Entity entity, PowerupConfig config, int currentLevel)
        {
            switch (config)
            {
                case PermanentTowerHealConfig towerHealConfig:
                    return new TowerHealAbility (entity, towerHealConfig, currentLevel);
                
                case PermanentDamageFirstEnemiesConfig damageFirstEnemiesConfig:
                    return new DamageFirstEnemiesAbility(entity, damageFirstEnemiesConfig, currentLevel, _entitiesLifeContext, _stageProviderService);
                
                case PermanentIncreaseNormalAbilityDamageConfig increaseDamageConfig:
                    return new IncreaseClickDamageAbility(entity, increaseDamageConfig, currentLevel);

                default:
                    throw new ArgumentException();
            }
        }
    }
}