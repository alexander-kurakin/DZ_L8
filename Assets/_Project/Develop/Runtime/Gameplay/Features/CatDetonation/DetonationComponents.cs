using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Utilities.Conditions;
using Assets._Project.Develop.Runtime.Utilities.Reactive;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.DistanceDetector
{
    public class InDetonateProcess : IEntityComponent
    {
        public ReactiveVariable<bool> Value;
    }

    public class StartTauntEvent : IEntityComponent
    {
        public ReactiveEvent Value;
    }

    public class CanStartDetonate : IEntityComponent
    {
        public ICompositeCondition Value;
    }

    public class DetonateTauntIndex : IEntityComponent
    {
        public ReactiveVariable<int> Value;
    }
    
    public class TauntFinishedEvent : IEntityComponent
    {
        public ReactiveEvent Value;
    }
    
    public class DetonateProcessCurrentTime : IEntityComponent
    {
        public ReactiveVariable<float> Value;
    }
    
    public class ExplosionCurrentTime : IEntityComponent
    {
        public ReactiveVariable<float> Value;
    }
    
    public class InExplosionProcess : IEntityComponent
    {
        public ReactiveVariable<bool> Value;
    }
    
    public class StartExplosionEvent : IEntityComponent
    {
        public ReactiveEvent Value;
    }
    
    public class HideExplosionSourceEvent : IEntityComponent
    {
        public ReactiveEvent Value;
    }
    
    public class HideExplosionSourceDelayTime : IEntityComponent
    {
        public ReactiveVariable<float> Value;
    }
}