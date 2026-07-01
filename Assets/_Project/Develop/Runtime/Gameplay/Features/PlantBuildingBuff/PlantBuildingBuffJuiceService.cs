using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;

using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;

using Assets._Project.Develop.Runtime.Gameplay.Features.JuiceFeature;

using Assets._Project.Develop.Runtime.Utilities.Audio;

using DG.Tweening;

using UnityEngine;



namespace Assets._Project.Develop.Runtime.Gameplay.Features.PlantBuildingBuff

{

    public class PlantBuildingBuffJuiceService

    {

        private const float ICE_FLOOR_FLAT_SCALE = 1f;



        private readonly PlantBuildingBuffService _plantBuildingBuffService;

        private readonly BuildingBuffCastAbilityConfig _lmbConfig;

        private readonly IGameSoundsService _gameSoundsService;



        public PlantBuildingBuffJuiceService(

            PlantBuildingBuffService plantBuildingBuffService,

            BuildingBuffCastAbilityConfig lmbConfig,

            IGameSoundsService gameSoundsService)

        {

            _plantBuildingBuffService = plantBuildingBuffService;

            _lmbConfig = lmbConfig;

            _gameSoundsService = gameSoundsService;

        }



        public bool IsBuffed(Entity plantEntity)

        {

            return _plantBuildingBuffService.IsBuffed(plantEntity);

        }



        public void PlayBuffedMinePulseFollowup(Vector3 worldPoint)

        {

            float delaySeconds = ResolveFollowupDelaySeconds();

            ScheduleFollowup(delaySeconds, () =>

            {

                SpawnIceFloorAt(worldPoint);

                PlayCrunchSound();

            });

        }



        public void PlayBuffedTurretHit()

        {

            // RegularImpact is already played by enemy TakeDamageView on turret hits.

            ScheduleCrunchSound();

        }



        public void PlayBuffedToxicTick()

        {

            ScheduleCrunchSound();

        }



        private void ScheduleCrunchSound()

        {

            float delaySeconds = ResolveFollowupDelaySeconds();

            ScheduleFollowup(delaySeconds, PlayCrunchSound);

        }



        private void PlayCrunchSound()

        {

            _gameSoundsService.PlayOneShot(GameSoundsIDs.CastIceSpell);

        }



        private void SpawnIceFloorAt(Vector3 worldPoint)

        {

            if (_lmbConfig.IceFloorVfxPrefab == null)

                return;



            GameObject instance = GameplayVfxUtility.SpawnAt(

                _lmbConfig.IceFloorVfxPrefab,

                worldPoint,

                Quaternion.identity,

                null,

                1f);



            if (instance == null)

                return;



            float flatScale = ResolveIceFloorFlatScale();

            instance.transform.localScale = new Vector3(flatScale, ICE_FLOOR_FLAT_SCALE, flatScale);

            GameplayVfxUtility.ScheduleDestroyAfterLifetime(instance);

        }



        private float ResolveIceFloorFlatScale()

        {

            float referenceDiameter = _lmbConfig.PreviewReferenceDiameter;



            if (referenceDiameter <= 0f)

                return _lmbConfig.CastVfxScaleMultiplier;



            float indicatorDiameter = _lmbConfig.PreviewIndicatorDiameter;

            return _lmbConfig.CastVfxScaleMultiplier * (indicatorDiameter / referenceDiameter);

        }



        private float ResolveFollowupDelaySeconds()

        {

            return _lmbConfig.BuildingBuffFollowupDelaySeconds;

        }



        private static void ScheduleFollowup(float delaySeconds, TweenCallback onComplete)

        {

            DOVirtual

                .DelayedCall(delaySeconds, onComplete)

                .Play();

        }

    }

}

