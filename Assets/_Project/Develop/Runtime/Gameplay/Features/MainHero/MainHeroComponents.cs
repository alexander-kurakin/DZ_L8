using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.MainHero
{
    public class IsMainHero : IEntityComponent
    {
    }

    public class ShootingPoint : IEntityComponent
    {
        public Transform Value;
    }

    public class CurrentProjectile : IEntityComponent
    {
        public ReactiveVariable<Entity> Value;
    }

    public class IsProjectileInHand : IEntityComponent
    {
        public ReactiveVariable<bool> Value;
    }
}
