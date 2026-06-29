using Assets._Project.Develop.Runtime.Configs.Gameplay.Juice;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.JuiceFeature
{
    public static class GameplayVfxUtility
    {
        private static float _defaultEffectLifetimeSeconds = 3f;
        private static float _effectLifetimePaddingSeconds = 0.35f;

        public static void Configure(GameplayVfxConfig config)
        {
            if (config == null)
                return;

            _defaultEffectLifetimeSeconds = config.DefaultEffectLifetimeSeconds;
            _effectLifetimePaddingSeconds = config.EffectLifetimePaddingSeconds;
        }

        public static GameObject SpawnAt(
            GameObject prefab,
            Vector3 worldPosition,
            Quaternion worldRotation,
            Transform parent = null,
            float uniformScale = 1f)
        {
            if (prefab == null)
                return null;

            GameObject instance = Object.Instantiate(prefab, worldPosition, worldRotation, parent);
            instance.SetActive(true);

            if (uniformScale != 1f)
                instance.transform.localScale = Vector3.one * uniformScale;

            PlayParticleSystemsInChildren(instance);
            return instance;
        }

        public static GameObject SpawnTransientAt(
            GameObject prefab,
            Vector3 worldPosition,
            Quaternion worldRotation,
            Transform parent = null,
            float uniformScale = 1f)
        {
            GameObject instance = SpawnAt(prefab, worldPosition, worldRotation, parent, uniformScale);
            ScheduleDestroyAfterLifetime(instance);
            return instance;
        }

        public static void ScheduleDestroyAfterLifetime(GameObject instance)
        {
            if (instance == null)
                return;

            Object.Destroy(instance, GetEffectLifetimeSeconds(instance));
        }

        public static float GetEffectLifetimeSeconds(GameObject root)
        {
            if (root == null)
                return _defaultEffectLifetimeSeconds;

            float maxLifetimeSeconds = _defaultEffectLifetimeSeconds;
            ParticleSystem[] particleSystems = root.GetComponentsInChildren<ParticleSystem>(true);

            for (int index = 0; index < particleSystems.Length; index++)
            {
                ParticleSystem.MainModule mainModule = particleSystems[index].main;

                if (mainModule.loop)
                    continue;

                float startLifetime = mainModule.startLifetime.constantMax;

                if (mainModule.startLifetime.mode == ParticleSystemCurveMode.TwoConstants)
                    startLifetime = mainModule.startLifetime.constantMax;

                float effectLifetimeSeconds = mainModule.duration + startLifetime;

                if (effectLifetimeSeconds > maxLifetimeSeconds)
                    maxLifetimeSeconds = effectLifetimeSeconds;
            }

            return maxLifetimeSeconds + _effectLifetimePaddingSeconds;
        }

        public static void PlayParticleSystemsInChildren(GameObject root)
        {
            ParticleSystem[] particleSystems = root.GetComponentsInChildren<ParticleSystem>(true);

            for (int index = 0; index < particleSystems.Length; index++)
            {
                ParticleSystem.MainModule mainModule = particleSystems[index].main;
                mainModule.scalingMode = ParticleSystemScalingMode.Hierarchy;
                particleSystems[index].Play(true);
            }
        }

        public static void PlayMovingProjectileParticles(GameObject root)
        {
            ParticleSystem[] particleSystems = root.GetComponentsInChildren<ParticleSystem>(true);

            for (int index = 0; index < particleSystems.Length; index++)
            {
                ParticleSystem particleSystem = particleSystems[index];
                ParticleSystem.MainModule mainModule = particleSystem.main;
                mainModule.scalingMode = ParticleSystemScalingMode.Hierarchy;
                mainModule.simulationSpace = ParticleSystemSimulationSpace.Local;
                particleSystem.Play(true);
            }
        }

        public static void PlayLoopingParticleSystemsInChildren(GameObject root)
        {
            ParticleSystem[] particleSystems = root.GetComponentsInChildren<ParticleSystem>(true);

            for (int index = 0; index < particleSystems.Length; index++)
            {
                ParticleSystem particleSystem = particleSystems[index];
                ParticleSystem.MainModule mainModule = particleSystem.main;
                mainModule.loop = true;
                mainModule.prewarm = true;
                mainModule.scalingMode = ParticleSystemScalingMode.Hierarchy;
                particleSystem.Play(true);
            }
        }
    }
}
