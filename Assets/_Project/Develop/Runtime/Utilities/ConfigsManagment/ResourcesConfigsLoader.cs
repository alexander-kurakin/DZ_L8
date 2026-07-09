using Assets._Project.Develop.Runtime.Configs.Gameplay.Combat;

using Assets._Project.Develop.Runtime.Configs.Gameplay.Camera;

using Assets._Project.Develop.Runtime.Configs.Gameplay.Throw;

using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;

using Assets._Project.Develop.Runtime.Configs.Gameplay.Levels;

using Assets._Project.Develop.Runtime.Configs.Gameplay.MouseConfig;

using Assets._Project.Develop.Runtime.Configs.Gameplay.ProjectileModifiers;

using Assets._Project.Develop.Runtime.Configs.Gameplay.Stages;

using Assets._Project.Develop.Runtime.Configs.Gameplay.Waves;

using Assets._Project.Develop.Runtime.Configs.Meta.Stats;

using Assets._Project.Develop.Runtime.Configs.Meta.Wallet;

using Assets._Project.Develop.Runtime.Configs.Utilities.Audio;

using Assets._Project.Develop.Runtime.Utilities.AssetsManagment;

using Assets._Project.Develop.Runtime.Utilities.ConfigsManagment;

using System;

using System.Collections;

using System.Collections.Generic;
using UnityEngine;



namespace Assets._Project.Develop.Runtime.Utilities.ConfigsManagment

{

    public class ResourcesConfigsLoader : IConfigsLoader

    {

        private readonly ResourcesAssetsLoader _resources;



        private readonly Dictionary<Type, string> _configsResourcesPaths = new()

        {

            { typeof(StartWalletConfig), "Configs/Meta/Wallet/StartWalletConfig" },

            { typeof(CurrencyIconsConfig), "Configs/Meta/Wallet/CurrencyIconsConfig" },

            { typeof(StatIconsConfig), "Configs/Meta/Stats/StatIconsConfig" },

            { typeof(LevelsListConfig), "Configs/Gameplay/Levels/LevelsListConfig" },

            { typeof(HeroConfig), "Configs/Gameplay/Entities/Characters/HeroConfig" },

            { typeof(CombatConfig), "Configs/Gameplay/Combat/CombatConfig" },

            { typeof(ThrowChargeConfig), "Configs/Gameplay/Throw/ThrowChargeConfig" },

            { typeof(HeroCameraConfig), "Configs/Gameplay/Camera/HeroCameraConfig" },

            { typeof(WaveProgressConfig), "Configs/Gameplay/Waves/WaveProgressConfig" },

            { typeof(RaycastConfig), "Configs/Gameplay/MouseConfig/RaycastConfig" },

            { typeof(ProjectileModifierIconsConfig), "Configs/Gameplay/ProjectileModifiers/ProjectileModifierIconsConfig" },

            { typeof(BackgroundMusicConfig), "Configs/Utilities/Audio/BackgroundMusicConfig" },

            { typeof(UISoundsConfig), "Configs/Utilities/Audio/UISoundsConfig" },

            { typeof(GameSoundsConfig), "Configs/Utilities/Audio/GameSoundsConfig" },

        };



        public ResourcesConfigsLoader(ResourcesAssetsLoader resources)

        {

            _resources = resources;

        }



        public IEnumerator LoadAsync(Action<Dictionary<Type, object>> onConfigsLoaded)

        {

            Dictionary<Type, object> loadedConfigs = new();



            foreach (KeyValuePair<Type, string> configResourcesPath in _configsResourcesPaths)

            {

                object config = LoadConfig(configResourcesPath.Key, configResourcesPath.Value);

                loadedConfigs.Add(configResourcesPath.Key, config);

                yield return null;

            }



            onConfigsLoaded?.Invoke(loadedConfigs);

        }



        private object LoadConfig(Type configType, string resourcePath)

        {

            ScriptableObject config = _resources.Load<ScriptableObject>(resourcePath);



            if (config == null)

            {

                throw new InvalidOperationException(

                    $"Resources config not found at '{resourcePath}' for type {configType.Name}.");

            }



            if (configType.IsInstanceOfType(config) == false)

            {

                throw new InvalidOperationException(

                    $"Resources config type mismatch at '{resourcePath}': expected {configType.Name}, got {config.GetType().Name}.");

            }



            return config;

        }

    }

}

