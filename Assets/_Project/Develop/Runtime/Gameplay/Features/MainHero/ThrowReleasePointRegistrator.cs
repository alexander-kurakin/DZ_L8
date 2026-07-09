using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.MainHero
{
    public class ThrowReleasePointRegistrator : MonoEntityRegistrator
    {
        [SerializeField] private Transform _throwReleasePoint;

        public override void Register(Entity entity)
        {
            entity.AddThrowReleasePoint(_throwReleasePoint);
        }
    }
}
