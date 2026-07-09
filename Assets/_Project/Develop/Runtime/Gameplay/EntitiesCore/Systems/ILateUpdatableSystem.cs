namespace Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems
{
    public interface ILateUpdatableSystem : IEntitySystem
    {
        void OnLateUpdate(float deltaTime);
    }
}
