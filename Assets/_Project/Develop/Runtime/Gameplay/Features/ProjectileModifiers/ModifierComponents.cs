using System.Collections.Generic;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Utilities.Reactive;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.ProjectileModifiers
{
    public class EquippedProjectileModifiers : IEntityComponent
    {
        public List<ModifierType> Value;
    }

    public class ActiveProjectileModifier : IEntityComponent
    {
        public ReactiveVariable<ModifierType> Value;
    }
}
