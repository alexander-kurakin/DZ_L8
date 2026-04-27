using Assets._Project.Develop.Runtime.Configs.Gameplay.Stages;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.Enemies;
using Assets._Project.Develop.Runtime.Infrastructure.DI;
using System;
using _Project.Develop.Runtime.Gameplay.Features.Input;
using _Project.Develop.Runtime.Meta.Features.Powerups;
using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.MainHero;
using Assets._Project.Develop.Runtime.Utilities.Audio;
using Assets._Project.Develop.Runtime.Utilities.ConfigsManagment;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature
{
    public class StagesFactory
    {
        private readonly DIContainer _container;

        public StagesFactory(DIContainer container)
        {
            _container = container;
        }

        public IStage Create(StageConfig stageConfig)
        {
            switch (stageConfig)
            {
                case ClearAllEnemiesStageConfig clearAllEnemiesStageConfig:
                    return new ClearAllEnemiesStage(
                        clearAllEnemiesStageConfig,
                        _container.Resolve<EnemiesFactory>(),
                        _container.Resolve<EntitiesLifeContext>(),
                        _container.Resolve<ConfigsProviderService>(),
                        _container.Resolve<MainHeroHolderService>(),
                        _container.Resolve<MouseInput>(),
                        _container.Resolve<MouseRaycastService>(),
                        _container.Resolve<IBackgroundMusicService>(),
                        _container.Resolve<PermanentPowerupResolver>());

                default:
                    throw new ArgumentException($"Not supported {stageConfig.GetType()} type config");
            }
        }
    }
}
