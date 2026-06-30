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
        public ReactiveEvent<Vector3> Value;
    }

    public class BrotherStoneThrowEvent : IEntityComponent
    {
        public ReactiveEvent<Entity> Value;
    }

    public class BrotherStoneThrowing : IEntityComponent
    {
        public ReactiveVariable<bool> Value;
    }

    public class BrotherStoneArcFlight : IEntityComponent
    {
        public Vector3 StartPosition;
        public Vector3 TargetPosition;
        public Entity TargetEntity;
        public float Speed;
        public float TotalDistance;
        public float TraveledTime;
        public bool IsCompleted;
    }
}