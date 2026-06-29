namespace Assets._Project.Develop.Runtime.Gameplay.Features.TakeDamage
{
    public enum TakeDamageVisualKind
    {
        Default = 0,
        SectorAbility = 1,
        Mine = 2,
    }

    public struct TakeDamageInfo
    {
        public float Damage;
        public TakeDamageVisualKind VisualKind;

        public TakeDamageInfo(float damage, TakeDamageVisualKind visualKind)
        {
            Damage = damage;
            VisualKind = visualKind;
        }
    }
}
