using System.Collections.Generic;
using Assets._Project.Develop.Runtime.Configs.Meta.Stats;
using Assets._Project.Develop.Runtime.UI.CommonViews;
using Assets._Project.Develop.Runtime.UI.Core;

namespace Assets._Project.Develop.Runtime.UI.Stats
{
    public class GameStatsPresenter : IPresenter
    {
        private readonly StatsService _statsService;
        private readonly ProjectPresentersFactory _presentersFactory;
        private readonly ViewsFactory _viewsFactory;

        private readonly IconTextListView _view;

        private readonly List<SingleGameStatPresenter> _gameStatsPresenters = new();

        public GameStatsPresenter(
            StatsService statsService,
            ProjectPresentersFactory presentersFactory,
            ViewsFactory viewsFactory,
            IconTextListView view)
        {
            _statsService = statsService;
            _presentersFactory = presentersFactory;
            _viewsFactory = viewsFactory;
            _view = view;
        }

        public void Initialize()
        {
            IconTextView winView = _viewsFactory.Create<IconTextView>(ViewIDs.StatIconTextView);
            _view.Add(winView);
            
            SingleGameStatPresenter winPresenter = _presentersFactory.CreateSingleGameStatPresenter(
                winView,
                _statsService.Wins,
                StatType.Wins);
            
            winPresenter.Initialize();
            _gameStatsPresenters.Add(winPresenter);

            IconTextView loseView = _viewsFactory.Create<IconTextView>(ViewIDs.StatIconTextView);
            _view.Add(loseView);
            
            SingleGameStatPresenter losePresenter = _presentersFactory.CreateSingleGameStatPresenter(
                loseView,
                _statsService.Losses,
                StatType.Losses);
            
            losePresenter.Initialize();
            _gameStatsPresenters.Add(losePresenter);            
        }

        public void Dispose()
        {
            foreach (SingleGameStatPresenter gameStatsPresenter in _gameStatsPresenters)
            {
                _view.Remove(gameStatsPresenter.View);
                _viewsFactory.Release(gameStatsPresenter.View);
                gameStatsPresenter.Dispose();
            }

            _gameStatsPresenters.Clear();
        }
    }
}