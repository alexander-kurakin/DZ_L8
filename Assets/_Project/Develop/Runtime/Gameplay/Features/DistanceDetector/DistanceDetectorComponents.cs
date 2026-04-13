using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Utilities.Reactive;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.DistanceDetector
{
    public class DistanceToTargetGoal : IEntityComponent
    {
        public ReactiveVariable<float> Value;
    }
    
    public class DistanceToTargetCurrent : IEntityComponent
    {
        public ReactiveVariable<float> Value;
    }        

    public class DistanceToTargetReachedEvent : IEntityComponent
    {
        public ReactiveEvent Value;
    }

    public class DistanceToTargetReached : IEntityComponent
    {
        public ReactiveVariable<bool> Value;
    }
}