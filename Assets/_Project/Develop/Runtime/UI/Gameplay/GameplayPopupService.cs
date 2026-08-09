using System;
using Assets._Project.Develop.Runtime.Gameplay.States;
using Assets._Project.Develop.Runtime.UI.Gameplay.ResultsPopups;
using Assets._Project.Develop.Runtime.UI;
using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.UI.QuitConfirmation;
using Assets._Project.Develop.Runtime.Utilities.ApplicationQuit;
using UnityEngine;

namespace _Project.Develop.Runtime.UI.Gameplay
{
    public class GameplayPopupService : PopupService, IQuitConfirmationOpener
    {
        private readonly GameplayUIRoot _gameplayUIRoot;
        private readonly GameplayPresentersFactory _gameplayPresentersFactory;
        private readonly ProjectPresentersFactory _projectPresentersFactory;
        private readonly EscToQuitService _escToQuitService;

        private QuitConfirmationPopupPresenter _quitConfirmationPopup;

        public GameplayPopupService(
            ViewsFactory viewsFactory,
            ProjectPresentersFactory presentersFactory,
            GameplayUIRoot gameplayUIRoot,
            GameplayPresentersFactory gameplayPresentersFactory,
            EscToQuitService escToQuitService)
                : base(viewsFactory, presentersFactory)
        {
            _gameplayUIRoot = gameplayUIRoot;
            _gameplayPresentersFactory = gameplayPresentersFactory;
            _projectPresentersFactory = presentersFactory;
            _escToQuitService = escToQuitService;

            _escToQuitService.Bind(this);
        }

        protected override Transform PopupLayer => _gameplayUIRoot.PopupsLayer;

        public WinPopupPresenter OpenWinPopup(RewardsData rewardsData, Action closedCallback = null)
        {
            WinPopupOpenArgs openArgs = new WinPopupOpenArgs
            {
                RewardsData = rewardsData,
                Mode = WinPopupMode.Standard
            };

            return OpenWinPopup(openArgs, closedCallback);
        }

        public WinPopupPresenter OpenWinPopup(WinPopupOpenArgs openArgs, Action closedCallback = null)
        {
            WinPopupView view = ViewsFactory.Create<WinPopupView>(ViewIDs.WinPopup, PopupLayer);

            WinPopupPresenter popup = _gameplayPresentersFactory.CreateWinPopupPresenter(view, openArgs);

            OnPopupCreated(popup, view, closedCallback);

            return popup;
        }

        public DefeatPopupPresenter OpenDefeatPopup(Action closedCallback = null)
        {
            DefeatPopupView view = ViewsFactory.Create<DefeatPopupView>(ViewIDs.DefeatPopup, PopupLayer);

            DefeatPopupPresenter popup = _gameplayPresentersFactory.CreateDefeatPopupPresenter(view);

            OnPopupCreated(popup, view, closedCallback);

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

            _quitConfirmationPopup = _projectPresentersFactory.CreateQuitConfirmationPopupPresenter(view);

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
