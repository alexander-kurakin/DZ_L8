using System;
using _Project.Develop.Runtime.Meta.Features.Powerups.Abilities;
using Assets._Project.Develop.Runtime.Configs.Meta.NewPowerups;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Infrastructure.DI;
using Assets._Project.Develop.Runtime.Meta.Features.Powerups;

namespace _Project.Develop.Runtime.Meta.Features.Powerups
{
    public class PowerupFactory
    {
        private DIContainer _container;

        public PowerupFactory(DIContainer container)
        {
            _container = container;
        }

        public Powerup CreatePowerupFor(Entity entity, PowerupConfig config, int currentLevel)
        {
            switch (config)
            {
                case PermanentTowerHealConfig towerHealConfig:
                    return new TowerHealAbility (entity, towerHealConfig, currentLevel);

                default:
                    throw new ArgumentException();
            }
        }
    }
}