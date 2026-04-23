using Assets._Project.Develop.Runtime.Configs.Meta.Stats;
using Assets._Project.Develop.Runtime.Infrastructure.DI;
using Assets._Project.Develop.Runtime.UI;

namespace _Project.Develop.Runtime.UI.Gameplay
{
    public class GameplayPresentersFactory
    {
        private readonly DIContainer _container;
        
        public GameplayPresentersFactory(DIContainer container)
        {
            _container = container;
        }

        public GameplayScreenPresenter CreateGameplayScreen(GameplayScreenView view)
        {
            return new GameplayScreenPresenter(
                _container.Resolve<ProjectPresentersFactory>(),
                view);
        }
    }
}