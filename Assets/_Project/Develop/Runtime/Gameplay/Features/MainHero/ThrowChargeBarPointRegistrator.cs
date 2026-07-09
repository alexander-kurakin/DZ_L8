using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.MainHero
{
    public class ThrowChargeBarPointRegistrator : MonoEntityRegistrator
    {
        [SerializeField] private Transform _point;

        public override void Register(Entity entity)
        {
            entity.AddThrowChargeBarPoint(_point);
        }
    }
}
