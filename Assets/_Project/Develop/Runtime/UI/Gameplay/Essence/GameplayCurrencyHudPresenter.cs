using Assets._Project.Develop.Runtime.Gameplay.Features.EssenceFeature;
using Assets._Project.Develop.Runtime.Meta.Features.Wallet;
using Assets._Project.Develop.Runtime.UI;
using Assets._Project.Develop.Runtime.UI.CommonViews;
using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.UI.Wallet;

namespace _Project.Develop.Runtime.UI.Gameplay.Essence
{
    public class GameplayCurrencyHudPresenter : IPresenter
    {
        private readonly WalletService _walletService;
        private readonly RunEssenceService _runEssenceService;
        private readonly ProjectPresentersFactory _presentersFactory;
        private readonly ViewsFactory _viewsFactory;
        private readonly IconTextListView _view;

        private CurrencyPresenter _goldPresenter;
        private CurrencyPresenter _essencePresenter;

        public GameplayCurrencyHudPresenter(
            WalletService walletService,
            RunEssenceService runEssenceService,
            ProjectPresentersFactory presentersFactory,
            ViewsFactory viewsFactory,
            IconTextListView view)
        {
            _walletService = walletService;
            _runEssenceService = runEssenceService;
            _presentersFactory = presentersFactory;
            _viewsFactory = viewsFactory;
            _view = view;
        }

        public void Initialize()
        {
            IconTextView goldView = _viewsFactory.Create<IconTextView>(ViewIDs.WalletIconTextView);
            _view.Add(goldView);

            _goldPresenter = _presentersFactory.CreateCurrencyPresenter(
                goldView,
                _walletService.GetCurrency(CurrencyTypes.Gold),
                CurrencyTypes.Gold);

            _goldPresenter.Initialize();

            IconTextView essenceView = _viewsFactory.Create<IconTextView>(ViewIDs.WalletIconTextView);
            _view.Add(essenceView);

            _essencePresenter = _presentersFactory.CreateCurrencyPresenter(
                essenceView,
                _runEssenceService.Amount,
                CurrencyTypes.Essence);

            _essencePresenter.Initialize();
        }

        public void Dispose()
        {
            _view.Remove(_goldPresenter.View);
            _viewsFactory.Release(_goldPresenter.View);
            _goldPresenter.Dispose();

            _view.Remove(_essencePresenter.View);
            _viewsFactory.Release(_essencePresenter.View);
            _essencePresenter.Dispose();
        }
    }
}
