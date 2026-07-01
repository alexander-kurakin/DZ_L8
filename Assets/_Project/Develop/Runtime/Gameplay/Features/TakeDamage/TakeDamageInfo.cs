using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.TakeDamage
{
    public enum TakeDamageVisualKind
    {
        Default = 0,
        SectorAbility = 1,
        Mine = 2,
        Turret = 3,
        Toxic = 4,
    }

    public struct TakeDamageInfo
    {
        public float Damage;
        public TakeDamageVisualKind VisualKind;
        public Entity Source;

        public TakeDamageInfo(float damage, TakeDamageVisualKind visualKind, Entity source = null)
        {
            Damage = damage;
            VisualKind = visualKind;
            Source = source;
        }
    }
}
