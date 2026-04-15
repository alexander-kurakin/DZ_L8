using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.TowerWalker
{
    public class SpawnPoint : IEntityComponent
    {
        public Transform Value;
    }

    public class FenceMask : IEntityComponent
    {
        public LayerMask Value;
    }

    public class IsTouchingFence : IEntityComponent
    {
        public ReactiveVariable<bool> Value;
    }
    
    public class MagicCastRequestedEvent : IEntityComponent
    {
        public ReactiveEvent Value;
    }
}