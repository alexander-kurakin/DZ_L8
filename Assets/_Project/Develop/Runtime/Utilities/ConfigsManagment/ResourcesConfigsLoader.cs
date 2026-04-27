using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Levels;
using Assets._Project.Develop.Runtime.Configs.Gameplay.MouseConfig;
using Assets._Project.Develop.Runtime.Configs.Meta.Wallet;
using Assets._Project.Develop.Runtime.Utilities.AssetsManagment;
using System;
using System.Collections;
using System.Collections.Generic;
using _Project.Develop.Runtime.Configs.Gameplay.Abilities;
using _Project.Develop.Runtime.Configs.Meta.Powerups;
using _Project.Develop.Runtime.Configs.Meta.Stats;
using _Project.Develop.Runtime.Configs.Utilities.Audio;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Utilities.ConfigsManagment
{
    public class ResourcesConfigsLoader : IConfigsLoader
    {
        private readonly ResourcesAssetsLoader _resources;

        private readonly Dictionary<Type, string> _configsResourcesPaths = new()
        {
            {typeof(StartWalletConfig), "Configs/Meta/Wallet/StartWalletConfig" },
            {typeof(CurrencyIconsConfig), "Configs/Meta/Wallet/CurrencyIconsConfig" },
            {typeof(StatIconsConfig), "Configs/Meta/Stats/StatIconsConfig" },
            {typeof(LevelsListConfig), "Configs/Gameplay/Levels/LevelsListConfig" },
            {typeof(TowerConfig), "Configs/Gameplay/Entities/Characters/TowerConfig" },
            {typeof(MineConfig), "Configs/Gameplay/Entities/Abilities/MineConfig" },
            {typeof(TurretConfig), "Configs/Gameplay/Entities/Abilities/TurretConfig" },
            {typeof(ToxicAreaConfig), "Configs/Gameplay/Entities/Abilities/ToxicAreaConfig" },
            {typeof(ContactTriggerConfig), "Configs/Gameplay/Entities/Characters/ContactTriggerConfig" },
            {typeof(RaycastConfig), "Configs/Gameplay/MouseConfig/RaycastConfig" },
            {typeof(ExplodeAtPointAbilityConfig), "Configs/Gameplay/Entities/Abilities/ExplodeAtPointAbilityConfig" },
            {typeof(BackgroundMusicConfig), "Configs/Utilities/Audio/BackgroundMusicConfig" },
            {typeof(UISoundsConfig), "Configs/Utilities/Audio/UISoundsConfig" },
            {typeof(AbilityIconsConfig), "Configs/Gameplay/Abilities/AbilityIconsConfig" },
            {typeof(PermanentPowerupsConfig),"Configs/Meta/Powerups/PermanentPowerupsConfig"}
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
                ScriptableObject config = _resources.Load<ScriptableObject>(configResourcesPath.Value);
                loadedConfigs.Add(configResourcesPath.Key, config);
                yield return null;
            }

            onConfigsLoaded?.Invoke(loadedConfigs);
        }
    }
}