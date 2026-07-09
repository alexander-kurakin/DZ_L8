using Assets._Project.Develop.Runtime.Configs.Gameplay.Stages;
using Assets._Project.Develop.Runtime.Gameplay.Features.GnomeFeature;
using Assets._Project.Develop.Runtime.Infrastructure.DI;
using Assets._Project.Develop.Runtime.Utilities.Audio;
using System;

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

            return CreateGnomeArenaStage();
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

        private IStage CreateGnomeArenaStage()
        {
            return new GnomeArenaStage(
                _container.Resolve<IBackgroundMusicService>(),
                _container.Resolve<GnomeWinConditionService>());
        }
    }
}
