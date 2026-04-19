using System.Collections.Generic;
using UnityEngine;

using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Utilities.Reactive;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Ability
{
    public class AbilityUserActiveAbility : IEntityComponent
    {
        public ReactiveVariable<AbilityType> Value;
    }
    
    public class AbilityUserAllAbilities : IEntityComponent
    {
        public Dictionary<AbilityType, Entity> Value;
    }

    public class AbilityOwner : IEntityComponent
    {
        public ReactiveVariable<Entity> Value;
    }

    public class AbilityTypeName : IEntityComponent
    {
        public ReactiveVariable<AbilityType> Value;
    }
    
    public class AbilityUseRequest : IEntityComponent
    {
        public ReactiveEvent<Vector3> Value;
    }
}