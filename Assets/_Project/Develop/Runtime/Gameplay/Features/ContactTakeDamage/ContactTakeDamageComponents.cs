using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.TakeDamage;
using Assets._Project.Develop.Runtime.Utilities.Reactive;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.ContactTakeDamage
{
    public class BodyContactDamage : IEntityComponent
    {
        public ReactiveVariable<float> Value;
    }

    public class ContactDamageVisualKind : IEntityComponent
    {
        public TakeDamageVisualKind Value;
    }

    public class ContactDamageOwner : IEntityComponent
    {
        public Entity Value;
    }
}
