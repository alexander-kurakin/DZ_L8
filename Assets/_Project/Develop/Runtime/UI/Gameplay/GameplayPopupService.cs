using Assets._Project.Develop.Runtime.UI;
using Assets._Project.Develop.Runtime.UI.Core;
using UnityEngine;

namespace _Project.Develop.Runtime.UI.Gameplay
{
    public class GameplayPopupService : PopupService
    {
        private readonly GameplayUIRoot _gameplayUIRoot;
        private readonly GameplayPresentersFactory _gameplayPresentersFactory;
        
        public GameplayPopupService(
            ViewsFactory viewsFactory, 
            ProjectPresentersFactory presentersFactory,
            GameplayUIRoot gameplayUIRoot,
            GameplayPresentersFactory gameplayPresentersFactory) 
                : base(viewsFactory, presentersFactory)
        {
            _gameplayUIRoot = gameplayUIRoot;
            _gameplayPresentersFactory = gameplayPresentersFactory;
        }

        protected override Transform PopupLayer => _gameplayUIRoot.PopupsLayer;
    }
}