using Assets._Project.Develop.Runtime.Meta.Features.Wallet;
using System.Collections.Generic;
using _Project.Develop.Runtime.Meta.Features.Powerups;

namespace Assets._Project.Develop.Runtime.Utilities.DataManagment
{
    public class PlayerData : ISaveData
    {
        public Dictionary<CurrencyTypes, int> WalletData;
        public int Wins;
        public int Losses;
        public Dictionary<PowerupType, bool> PowerupsData;
        
    }
}
