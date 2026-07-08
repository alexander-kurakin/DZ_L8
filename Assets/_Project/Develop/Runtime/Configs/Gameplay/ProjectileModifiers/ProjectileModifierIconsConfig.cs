using System;
using Assets._Project.Develop.Runtime.Gameplay.Features.ProjectileModifiers;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.ProjectileModifiers
{
    [CreateAssetMenu(
        menuName = "Configs/Gameplay/ProjectileModifiers/NewProjectileModifierIconsConfig",
        fileName = "ProjectileModifierIconsConfig")]
    public class ProjectileModifierIconsConfig : ScriptableObject
    {
        [SerializeField] private Sprite _modifierAIcon;
        [SerializeField] private Sprite _modifierBIcon;
        [SerializeField] private Sprite _modifierCIcon;

        public Sprite GetSpriteFor(ModifierType modifierType)
        {
            switch (modifierType)
            {
                case ModifierType.ModifierA:
                    return _modifierAIcon;

                case ModifierType.ModifierB:
                    return _modifierBIcon;

                case ModifierType.ModifierC:
                    return _modifierCIcon;

                default:
                    throw new ArgumentOutOfRangeException(nameof(modifierType), modifierType, null);
            }
        }
    }
}
