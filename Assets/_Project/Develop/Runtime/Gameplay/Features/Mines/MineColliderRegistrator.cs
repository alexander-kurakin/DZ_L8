using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Mines
{
    public class MineColliderRegistrator : MonoEntityRegistrator
    {
        [SerializeField] private SphereCollider _sphericalBody;

        public override void Register(Entity entity)
        {
            entity.AddMineCollider(_sphericalBody);
        }
    }
}