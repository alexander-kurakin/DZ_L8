using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.MainHero
{
    public class ShootingPointEntityRegistrator : MonoEntityRegistrator
    {
        [SerializeField] private Transform _shootingPoint;

        public override void Register(Entity entity)
        {
            entity.AddShootingPoint(_shootingPoint);
        }
    }
}
