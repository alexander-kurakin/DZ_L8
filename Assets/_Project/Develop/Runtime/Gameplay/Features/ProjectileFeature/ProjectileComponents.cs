using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Utilities.Reactive;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.ProjectileFeature
{
    public class ProjectileSpeed : IEntityComponent
    {
        public ReactiveVariable<float> Value;
    }

    public class ProjectileDamage : IEntityComponent
    {
        public ReactiveVariable<float> Value;
    }

    public class ProjectilePierceCount : IEntityComponent
    {
        public ReactiveVariable<int> Value;
    }

    public class ProjectileOwner : IEntityComponent
    {
        public Entity Value;
    }
}
