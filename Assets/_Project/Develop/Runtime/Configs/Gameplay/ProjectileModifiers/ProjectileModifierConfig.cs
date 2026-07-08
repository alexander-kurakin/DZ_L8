using Assets._Project.Develop.Runtime.Gameplay.Features.ProjectileModifiers;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.ProjectileModifiers
{
    [CreateAssetMenu(
        menuName = "Configs/Gameplay/ProjectileModifiers/NewProjectileModifierConfig",
        fileName = "ProjectileModifierConfig")]
    public class ProjectileModifierConfig : ScriptableObject
    {
        [field: SerializeField] public ModifierType Type { get; private set; }
        [field: SerializeField, Min(0)] public float DamageMultiplier { get; private set; } = 1f;
        [field: SerializeField, Min(0)] public float ProjectileSpeed { get; private set; } = 25f;
    }
}
