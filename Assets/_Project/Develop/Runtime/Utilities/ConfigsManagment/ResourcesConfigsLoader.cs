using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Levels;
using Assets._Project.Develop.Runtime.Configs.Gameplay.MouseConfig;
using Assets._Project.Develop.Runtime.Configs.Meta.Wallet;
using Assets._Project.Develop.Runtime.Utilities.AssetsManagment;
using System;
using System.Collections;
using System.Collections.Generic;
using _Project.Develop.Runtime.Configs.Utilities.Audio;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Abilities;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Essence;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Sectors;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Spellcore;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Stages;
using Assets._Project.Develop.Runtime.Configs.Meta.NewPowerups;
using Assets._Project.Develop.Runtime.Configs.Meta.Stats;
using Assets._Project.Develop.Runtime.Configs.Utilities.Audio;
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
            {typeof(SectorGridConfig), "Configs/Gameplay/Sectors/SectorGridConfig" },
            {typeof(SectorVisualConfig), "Configs/Gameplay/Sectors/SectorVisualConfig" },
            {typeof(SpellcoreProgressionConfig), "Configs/Gameplay/Spellcore/SpellcoreProgressionConfig" },
            {typeof(EssenceConfig), "Configs/Gameplay/Essence/EssenceConfig" },
            {typeof(WaveEnemyPreviewIconsConfig), "Configs/Gameplay/Stages/WaveEnemyPreviewIconsConfig" },
            {typeof(ExplodeAtPointAbilityConfig), "Configs/Gameplay/Entities/Abilities/ExplodeAtPointAbilityConfig" },
            {typeof(BackgroundMusicConfig), "Configs/Utilities/Audio/BackgroundMusicConfig" },
            {typeof(UISoundsConfig), "Configs/Utilities/Audio/UISoundsConfig" },
            {typeof(GameSoundsConfig), "Configs/Utilities/Audio/GameSoundsConfig" },
            {typeof(AbilityIconsConfig), "Configs/Gameplay/Abilities/AbilityIconsConfig" },
            {typeof(PowerupConfigsContainer), "Configs/Gameplay/Abilities/PowerupConfigsContainer" },
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