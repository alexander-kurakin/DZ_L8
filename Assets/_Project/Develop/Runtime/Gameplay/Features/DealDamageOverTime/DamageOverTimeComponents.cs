using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Utilities.Reactive;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.DealDamageOverTime
{
    public class DamagePerTick : IEntityComponent
    {
        public ReactiveVariable<float> Value;
    }
    
    public class DamageInterval : IEntityComponent
    {
        public ReactiveVariable<float> Value;
    }
    
    public class DamageTimer : IEntityComponent
    {
        public ReactiveVariable<float> Value;
    }
}