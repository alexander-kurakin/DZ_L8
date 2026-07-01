using Assets._Project.Develop.Runtime.Configs.Gameplay.Stages;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.Enemies;
using Assets._Project.Develop.Runtime.Infrastructure.DI;
using System;
using _Project.Develop.Runtime.Gameplay.Features.Input;
using _Project.Develop.Runtime.Gameplay.Features.InputFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.MainHero;
using Assets._Project.Develop.Runtime.Gameplay.Features.PlantPlacementFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature;
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

        public IStage Create(ClearAllEnemiesWaveRuntimeData waveData)
        {
            if (waveData == null)
                throw new ArgumentNullException(nameof(waveData));

            return new ClearAllEnemiesStage(
                waveData,
                _container.Resolve<EnemiesFactory>(),
                _container.Resolve<EntitiesLifeContext>(),
                _container.Resolve<ConfigsProviderService>(),
                _container.Resolve<MainHeroHolderService>(),
                _container.Resolve<IMouseInputService>(),
                _container.Resolve<MouseRaycastService>(),
                _container.Resolve<MouseOverUIService>(),
                _container.Resolve<PlantSellInputService>(),
                _container.Resolve<IBackgroundMusicService>(),
                _container.Resolve<SectorRegistryService>(),
                _container.Resolve<WaveSpawnPlanService>());
        }

        public IStage Create(StageConfig stageConfig)
        {
            if (stageConfig == null)
                throw new ArgumentNullException(nameof(stageConfig));

            switch (stageConfig)
            {
                case ClearAllEnemiesStageConfig clearAllEnemiesStageConfig:
                    return Create(ClearAllEnemiesWaveRuntimeData.FromConfig(clearAllEnemiesStageConfig));

                default:
                    throw new ArgumentException($"Not supported {stageConfig.GetType().Name} type config");
            }
        }
    }
}
