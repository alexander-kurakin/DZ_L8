using Assets._Project.Develop.Runtime.Gameplay.Features.TakeDamage;
using Assets._Project.Develop.Runtime.Gameplay.Features.TeamsFeature;
using Assets._Project.Develop.Runtime.Utilities.Reactive;

namespace Assets._Project.Develop.Runtime.Gameplay.EntitiesCore
{
    public class EntitiesHelper
    {
        public static bool TryTakeDamageFrom(
            Entity source,
            Entity damageable,
            float damage,
            TakeDamageVisualKind visualKind = TakeDamageVisualKind.Default)
        {
            if (damageable.TryGetTakeDamageRequest(out ReactiveEvent<TakeDamageInfo> takeDamageRequest) == false)
                return false;

            if (source.TryGetTeam(out ReactiveVariable<Teams> sourceTeam)
                && damageable.TryGetTeam(out ReactiveVariable<Teams> damageableTeam))
            {
                if (sourceTeam.Value == damageableTeam.Value)
                    return false;
            }

            takeDamageRequest.Invoke(new TakeDamageInfo(damage, visualKind, source));
            return true;
        }
    }
}
