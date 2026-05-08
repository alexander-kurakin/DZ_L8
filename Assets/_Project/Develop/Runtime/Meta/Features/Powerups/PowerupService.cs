using System;
using System.Collections.Generic;
using Assets._Project.Develop.Runtime.Configs.Meta.NewPowerups;
using Assets._Project.Develop.Runtime.Utilities.DataManagment;
using Assets._Project.Develop.Runtime.Utilities.DataManagment.DataProviders;
using Assets._Project.Develop.Runtime.Utilities.Reactive;

namespace _Project.Develop.Runtime.Meta.Features.Powerups
{
    public class PowerupService : IDataReader<PlayerData>, IDataWriter<PlayerData>
    {
        private readonly Dictionary<string, PowerupSaveData> _powerupData;
        
        public PowerupService(PlayerDataProvider playerDataProvider)
        {
            _powerupData = new Dictionary<string, PowerupSaveData>();
            
            playerDataProvider.RegisterWriter(this);
            playerDataProvider.RegisterReader(this);
        }

        public IReadOnlyList<PowerupUIData> GetPowerupUIData(IReadOnlyList<PowerupConfig> powerupConfigs)
        {
            List<PowerupUIData> powerupUIData = new(powerupConfigs.Count);

            foreach (PowerupConfig powerupConfig in powerupConfigs)
            {
                PowerupSaveData powerupSaveData = _powerupData[powerupConfig.ID];

                powerupUIData.Add(new PowerupUIData(
                    powerupConfig, 
                    powerupSaveData.Level, 
                    powerupSaveData.Unlocked));
            }
            
            return powerupUIData;
        }

        public PowerupSaveData GetPowerupDataByID(string ID)
        {
            PowerupSaveData powerupSaveData = _powerupData[ID];

            return new PowerupSaveData
            {
                Level = powerupSaveData.Level,
                Unlocked = powerupSaveData.Unlocked,
            };
        }

        public void UnlockPowerup(string ID)
        {
            PowerupSaveData powerupSaveData = _powerupData[ID];
            
            if (powerupSaveData.Unlocked)
                throw new InvalidOperationException($"Powerup '{ID}' already unlocked.");
            
            powerupSaveData.Unlocked = true;
            powerupSaveData.Level = 1;
        }

        public void UpgradePowerup(string ID, int maxLevel)
        {
            PowerupSaveData powerupSaveData = _powerupData[ID];
            
            if (powerupSaveData.Level >= maxLevel)
                throw new InvalidOperationException($"Powerup '{ID}' already max level.");
            
            powerupSaveData.Level++;
        }
        
        public void ReadFrom(PlayerData data)
        {
            foreach (KeyValuePair<string, PowerupSaveData> kvp in data.PowerupsData)
            {
                if (_powerupData.ContainsKey(kvp.Key))
                {
                    _powerupData[kvp.Key].Level = kvp.Value.Level;
                    _powerupData[kvp.Key].Unlocked = kvp.Value.Unlocked;
                }
                else
                {
                    _powerupData.Add(kvp.Key, new PowerupSaveData
                    {
                        Level = kvp.Value.Level,
                        Unlocked = kvp.Value.Unlocked
                    });                    
                }
            }

        }

        public void WriteTo(PlayerData data)
        {
            foreach (KeyValuePair<string, PowerupSaveData> kvp in _powerupData)
            {
                if (data.PowerupsData.ContainsKey(kvp.Key))
                {
                    data.PowerupsData[kvp.Key].Level = kvp.Value.Level;
                    data.PowerupsData[kvp.Key].Unlocked = kvp.Value.Unlocked;
                }
                else
                {
                    data.PowerupsData.Add(kvp.Key, new PowerupSaveData
                    {
                        Level = kvp.Value.Level,
                        Unlocked = kvp.Value.Unlocked
                    });
                }
            }
        }
    }
    
    public class PowerupSaveData
    {
        public int Level;
        public bool Unlocked;
    }
    
    public class PowerupUIData
    {
        public PowerupUIData(PowerupConfig config, int level, bool unlocked)
        {
            Config = config;
            Level = level;
            Unlocked = unlocked;
        }
        public PowerupConfig Config { get; }
        public int Level { get; }
        public bool Unlocked { get; }
    }
}