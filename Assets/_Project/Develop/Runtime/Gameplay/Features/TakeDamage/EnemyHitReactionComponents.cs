using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.TakeDamage
{
    public class EnemySpawnOrigin : IEntityComponent
    {
        public Vector3 Value;
    }

    public class EnemyHitStunRemainingTime : IEntityComponent
    {
        public ReactiveVariable<float> Value;
    }
}
