using Assets._Project.Develop.Runtime.Utilities.CoroutinesManagment;
using Assets._Project.Develop.Runtime.Utilities.DataManagment.DataProviders;

namespace Assets._Project.Develop.Runtime.Meta.Features.Wallet
{
    public class PersistedGoldRewardService
    {
        private readonly WalletService _walletService;
        private readonly PlayerDataProvider _playerDataProvider;
        private readonly ICoroutinesPerformer _coroutinesPerformer;

        public PersistedGoldRewardService(
            WalletService walletService,
            PlayerDataProvider playerDataProvider,
            ICoroutinesPerformer coroutinesPerformer)
        {
            _walletService = walletService;
            _playerDataProvider = playerDataProvider;
            _coroutinesPerformer = coroutinesPerformer;
        }

        public int AddGold(int goldAmount)
        {
            if (goldAmount <= 0)
                return 0;

            _walletService.Add(CurrencyTypes.Gold, goldAmount);
            return goldAmount;
        }

        public void PersistWallet()
        {
            _coroutinesPerformer.StartPerform(_playerDataProvider.SaveAsync());
        }

        public int AddGoldAndPersist(int goldAmount)
        {
            int grantedGold = AddGold(goldAmount);

            if (grantedGold > 0)
                PersistWallet();

            return grantedGold;
        }
    }
}
