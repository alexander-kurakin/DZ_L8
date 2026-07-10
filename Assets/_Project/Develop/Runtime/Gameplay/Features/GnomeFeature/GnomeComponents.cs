using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.GnomeFeature
{
    public class IsGnome : IEntityComponent
    {
    }

    public class IsPeeking : IEntityComponent
    {
        public ReactiveVariable<bool> Value;
    }

    public class GnomeHitCount : IEntityComponent
    {
        public ReactiveVariable<int> Value;
    }

    public class GnomeRequiredHits : IEntityComponent
    {
        public int Value;
    }

    public class GnomePeekOffset : IEntityComponent
    {
        public float Value;
    }

    public class GnomePeekDirection : IEntityComponent
    {
        public Vector3 Value;
    }

    public class GnomePeekLeanAngle : IEntityComponent
    {
        public float Value;
    }

    public class GnomeForcePeekRequested : IEntityComponent
    {
        public ReactiveVariable<bool> Value;
    }

    public class GnomeDeathDissolveDuration : IEntityComponent
    {
        public float Value;
    }

    public class GnomeIsVerticalLayout : IEntityComponent
    {
        public bool Value;
    }
}
