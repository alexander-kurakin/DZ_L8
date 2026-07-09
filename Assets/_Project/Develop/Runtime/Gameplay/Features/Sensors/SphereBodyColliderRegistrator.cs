using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Sensors
{
    public class SphereBodyColliderRegistrator : MonoEntityRegistrator
    {
        [SerializeField] private SphereCollider _body;

        public override void Register(Entity entity)
        {
            entity.AddSphereBodyCollider(_body);
        }
    }
}
