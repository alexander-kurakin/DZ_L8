using System;
using System.Collections.Generic;
using Assets._Project.Develop.Runtime.Utilities.DataManagment;
using Assets._Project.Develop.Runtime.Utilities.DataManagment.DataProviders;
using Assets._Project.Develop.Runtime.Utilities.Reactive;

namespace _Project.Develop.Runtime.Meta.Features.Powerups
{
    public class PowerupService : IDataReader<PlayerData>, IDataWriter<PlayerData>
    {
        private readonly Dictionary<PowerupType, ReactiveVariable<bool>> _powerups;
        
        public PowerupService(PlayerDataProvider playerDataProvider)
        {
            _powerups = new Dictionary<PowerupType, ReactiveVariable<bool>>();

            foreach (PowerupType powerupType in Enum.GetValues(typeof(PowerupType)))
                _powerups[powerupType] = new ReactiveVariable<bool>();
            
            playerDataProvider.RegisterWriter(this);
            playerDataProvider.RegisterReader(this);
        }
        
        public IReadOnlyVariable<bool> GetBy(PowerupType type) => _powerups[type];
        
        public void Set(PowerupType type, bool value) => _powerups[type].Value = value;
        
        public void ReadFrom(PlayerData data)
        {
            foreach (PowerupType powerupType in Enum.GetValues(typeof(PowerupType)))
            {
                if (data.PowerupsData != null && data.PowerupsData.TryGetValue(powerupType, out bool powerup))
                    _powerups[powerupType].Value = powerup;
                else
                    _powerups[powerupType].Value = false;
            }
        }

        public void WriteTo(PlayerData data)
        {
            if (data.PowerupsData == null)
                data.PowerupsData = new Dictionary<PowerupType, bool>();

            foreach (KeyValuePair<PowerupType, ReactiveVariable<bool>> keyValuePair in _powerups)
                data.PowerupsData[keyValuePair.Key] = keyValuePair.Value.Value;
                
        }
    }
}