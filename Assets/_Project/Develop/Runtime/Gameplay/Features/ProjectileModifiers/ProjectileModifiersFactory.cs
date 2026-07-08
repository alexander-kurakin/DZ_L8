using System.Collections.Generic;
using Assets._Project.Develop.Runtime.Configs.Gameplay.ProjectileModifiers;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.ProjectileFeature;
using Assets._Project.Develop.Runtime.Infrastructure.DI;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.ProjectileModifiers
{
    public class ProjectileModifiersFactory
    {
        private readonly Dictionary<ModifierType, ProjectileModifierConfig> _configsByType = new();

        public ProjectileModifiersFactory(DIContainer container)
        {
            CacheConfigs();
        }

        public IProjectileModifierBehavior CreateBehavior(ModifierType modifierType)
        {
            ProjectileModifierConfig config = GetConfig(modifierType);
            return new DefaultProjectileModifierBehavior(modifierType, config);
        }

        public void EquipAllModifiers(Entity hero)
        {
            hero.AddComponent(new EquippedProjectileModifiers
            {
                Value = new List<ModifierType>
                {
                    ModifierType.ModifierA,
                    ModifierType.ModifierB,
                    ModifierType.ModifierC,
                },
            });

            hero.AddComponent(new ActiveProjectileModifier
            {
                Value = new ReactiveVariable<ModifierType>(ModifierType.ModifierA),
            });
        }

        private void CacheConfigs()
        {
            foreach (ModifierType modifierType in System.Enum.GetValues(typeof(ModifierType)))
            {
                string resourcePath = $"Configs/Gameplay/ProjectileModifiers/{modifierType}Config";
                ProjectileModifierConfig config = Resources.Load<ProjectileModifierConfig>(resourcePath);

                if (config != null)
                    _configsByType[modifierType] = config;
            }
        }

        private ProjectileModifierConfig GetConfig(ModifierType modifierType)
        {
            if (_configsByType.TryGetValue(modifierType, out ProjectileModifierConfig config))
                return config;

            return null;
        }

        private sealed class DefaultProjectileModifierBehavior : IProjectileModifierBehavior
        {
            private readonly ModifierType _modifierType;
            private readonly ProjectileModifierConfig _config;

            public DefaultProjectileModifierBehavior(ModifierType modifierType, ProjectileModifierConfig config)
            {
                _modifierType = modifierType;
                _config = config;
            }

            public ModifierType Type => _modifierType;

            public void ApplyToProjectile(Entity projectile, float baseDamage, float baseSpeed)
            {
                float damageMultiplier = _config != null ? _config.DamageMultiplier : 1f;
                float projectileSpeed = _config != null && _config.ProjectileSpeed > 0f
                    ? _config.ProjectileSpeed
                    : baseSpeed;

                projectile
                    .AddProjectileDamage(new ReactiveVariable<float>(baseDamage * damageMultiplier))
                    .AddProjectileSpeed(new ReactiveVariable<float>(projectileSpeed));
            }
        }
    }
}
