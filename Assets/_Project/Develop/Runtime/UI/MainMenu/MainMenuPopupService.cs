using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.UI.MainMenu.ShopPopup;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.UI.MainMenu
{
    public class MainMenuPopupService : PopupService
    {
        private readonly MainMenuUIRoot _uiRoot;
        private readonly ProjectPresentersFactory _presentersFactory;
        private readonly MainMenuPresentersFactory _mainMenuPresentersFactory;

        public MainMenuPopupService(
            ViewsFactory viewsFactory,
            ProjectPresentersFactory presentersFactory,
            MainMenuPresentersFactory mainMenuPresentersFactory,
            MainMenuUIRoot uiRoot)
            : base(viewsFactory, presentersFactory)
        {
            _uiRoot = uiRoot;
            _presentersFactory = presentersFactory;
            _mainMenuPresentersFactory = mainMenuPresentersFactory;
        }

        protected override Transform PopupLayer => _uiRoot.PopupsLayer;
        
        public ShopPopupPresenter OpenShopPopup()
        {
            ShopPopupView view = ViewsFactory.Create<ShopPopupView>(ViewIDs.ShopPopup, PopupLayer);

            ShopPopupPresenter popup = _mainMenuPresentersFactory.CreateShopPopupPresenter(view);

            OnPopupCreated(popup, view);

            return popup;
        }
    }
}
