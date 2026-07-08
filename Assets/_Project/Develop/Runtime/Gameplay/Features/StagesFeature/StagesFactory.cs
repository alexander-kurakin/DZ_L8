using Assets._Project.Develop.Runtime.Configs.Gameplay.Stages;

using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;

using Assets._Project.Develop.Runtime.Infrastructure.DI;

using Assets._Project.Develop.Runtime.Utilities.Audio;

using Assets._Project.Develop.Runtime.Utilities.Reactive;

using System;

using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;



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

                _container.Resolve<IMouseInputService>(),

                _container.Resolve<IBackgroundMusicService>());

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

