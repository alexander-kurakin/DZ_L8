using Assets._Project.Develop.Runtime.Infrastructure.DI;

namespace Assets._Project.Develop.Runtime.Gameplay.EntitiesCore
{
    public class EntitiesFactory
    {
        public EntitiesFactory(DIContainer container)
        {
        }

        public Entity CreateEmpty() => new Entity();
    }
}
