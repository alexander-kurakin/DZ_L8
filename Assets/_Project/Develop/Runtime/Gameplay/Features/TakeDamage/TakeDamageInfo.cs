using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.TakeDamage
{
    public struct TakeDamageInfo
    {
        public float Damage;
        public Entity Source;

        public TakeDamageInfo(float damage, Entity source = null)
        {
            Damage = damage;
            Source = source;
        }
    }
}
