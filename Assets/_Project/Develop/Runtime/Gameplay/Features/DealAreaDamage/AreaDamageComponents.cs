using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Utilities;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.DealAreaDamage
{
    public class AreaImpactDamage : IEntityComponent
    {
        public ReactiveVariable<float> Value;
    }

    public class AreaImpactRadius : IEntityComponent
    {
        public ReactiveVariable<float> Value;
    }
    
    public class AreaImpactMask : IEntityComponent
    {
        public LayerMask Value;
    }
    
    public class AreaImpactCollidersBuffer : IEntityComponent
    {
        public Buffer<Collider> Value;
    }

    public class AreaImpactEntitiesBuffer : IEntityComponent
    {
        public Buffer<Entity> Value;
    }
    
    public class DealAreaImpactDamageRequest : IEntityComponent
    {
        public ReactiveEvent<Vector3> Value;
    }    
}