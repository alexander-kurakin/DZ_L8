using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.TakeDamage;
using Assets._Project.Develop.Runtime.Utilities.Conditions;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.ContactTakeDamage
{
    public static class ProjectileContactRules
    {
        public static bool ShouldIgnoreContact(Entity contact)
        {
            if (contact == null)
                return true;

            if (contact.HasComponent<BodyContactDamage>())
                return true;

            return false;
        }

        public static bool IsValidDamageTarget(Entity contact)
        {
            if (ShouldIgnoreContact(contact))
                return false;

            if (contact.HasComponent<TakeDamageRequest>() == false)
                return false;

            if (contact.TryGetCanTakeDamage(out ICompositeCondition canTakeDamage)
                && canTakeDamage.Evaluate() == false)
            {
                return false;
            }

            return true;
        }
    }
}
