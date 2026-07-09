using Assets._Project.Develop.Runtime.Gameplay.Features.TakeDamage;
using Assets._Project.Develop.Runtime.Utilities.Reactive;

namespace Assets._Project.Develop.Runtime.Gameplay.EntitiesCore
{
    public static class EntitiesHelper
    {
        public static bool TryTakeDamageFrom(Entity source, Entity target, float damage)
        {
            if (target.TryGetTakeDamageRequest(out ReactiveEvent<TakeDamageInfo> damageRequest) == false)
                return false;

            damageRequest.Invoke(new TakeDamageInfo(damage, source));
            return true;
        }
    }
}
