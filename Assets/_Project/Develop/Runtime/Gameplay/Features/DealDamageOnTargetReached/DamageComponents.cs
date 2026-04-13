using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Utilities.Reactive;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.DealDamageOnTargetReached
{
    public class ExplosionDamage : IEntityComponent
    {
        public ReactiveVariable<float> Value;
    }
}