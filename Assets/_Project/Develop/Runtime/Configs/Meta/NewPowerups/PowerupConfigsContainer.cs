using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Meta.NewPowerups
{
    [CreateAssetMenu(menuName = "Configs/Gameplay/Abilities/NewPowerupConfigsContainer", fileName = "PowerupConfigsContainer")]
    public class PowerupConfigsContainer : ScriptableObject
    {
        [SerializeField] private List<PowerupConfig> _powerupConfigs;

        public IReadOnlyList<PowerupConfig> PowerupConfigs => _powerupConfigs;

        public PowerupConfig GetConfigBy(string ID) => _powerupConfigs.First(config => config.ID == ID);
    }
}