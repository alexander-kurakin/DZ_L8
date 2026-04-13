using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Mines
{
    public class MineCollider : IEntityComponent
    {
        public SphereCollider Value;
    }
    
    public class MineDamage : IEntityComponent
    {
        public ReactiveVariable<float> Value;
    }
    
    public class MineExplosionRadius : IEntityComponent
    {
        public ReactiveVariable<float> Value;
    }
    
    public class MineDamageableMask : IEntityComponent
    {
        public LayerMask Value;
    }
    
}