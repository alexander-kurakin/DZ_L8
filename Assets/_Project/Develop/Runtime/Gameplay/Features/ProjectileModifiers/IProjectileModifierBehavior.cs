using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Utilities.Reactive;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.ProjectileModifiers
{
    public interface IProjectileModifierBehavior
    {
        ModifierType Type { get; }

        void ApplyToProjectile(Entity projectile, float baseDamage, float baseSpeed);
    }
}
