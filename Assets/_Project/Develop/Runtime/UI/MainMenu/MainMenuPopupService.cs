using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.UI.MainMenu.ShopPopup;
using Assets._Project.Develop.Runtime.UI.QuitConfirmation;
using Assets._Project.Develop.Runtime.Utilities.ApplicationQuit;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.UI.MainMenu
{
    public class MainMenuPopupService : PopupService, IQuitConfirmationOpener
    {
        private readonly MainMenuUIRoot _uiRoot;
        private readonly ProjectPresentersFactory _presentersFactory;
        private readonly MainMenuPresentersFactory _mainMenuPresentersFactory;
        private readonly EscToQuitService _escToQuitService;

        private QuitConfirmationPopupPresenter _quitConfirmationPopup;

        public MainMenuPopupService(
            ViewsFactory viewsFactory,
            ProjectPresentersFactory presentersFactory,
            MainMenuPresentersFactory mainMenuPresentersFactory,
            MainMenuUIRoot uiRoot,
            EscToQuitService escToQuitService)
            : base(viewsFactory, presentersFactory)
        {
            _uiRoot = uiRoot;
            _presentersFactory = presentersFactory;
            _mainMenuPresentersFactory = mainMenuPresentersFactory;
            _escToQuitService = escToQuitService;

            _escToQuitService.Bind(this);
        }

        protected override Transform PopupLayer => _uiRoot.PopupsLayer;

        public ShopPopupPresenter OpenShopPopup()
        {
            ShopPopupView view = ViewsFactory.Create<ShopPopupView>(ViewIDs.ShopPopup, PopupLayer);

            ShopPopupPresenter popup = _mainMenuPresentersFactory.CreateShopPopupPresenter(view);

            OnPopupCreated(popup, view);

            return popup;
        }

        public void OpenQuitConfirmation()
        {
            if (_quitConfirmationPopup != null)
            {
                ClosePopup(_quitConfirmationPopup);
                return;
            }

            QuitConfirmationPopupView view = ViewsFactory.Create<QuitConfirmationPopupView>(
                ViewIDs.QuitConfirmationPopup,
                PopupLayer);

            _quitConfirmationPopup = _presentersFactory.CreateQuitConfirmationPopupPresenter(view);

            OnPopupCreated(_quitConfirmationPopup, view, OnQuitConfirmationClosed);
        }

        public override void Dispose()
        {
            _escToQuitService.Unbind(this);

            base.Dispose();
        }

        private void OnQuitConfirmationClosed()
        {
            _quitConfirmationPopup = null;
        }
    }
}
