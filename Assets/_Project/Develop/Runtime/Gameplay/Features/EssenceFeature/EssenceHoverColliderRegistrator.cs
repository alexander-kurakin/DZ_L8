using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.EssenceFeature
{
    public class EssenceHoverColliderRegistrator : MonoEntityRegistrator
    {
        [SerializeField] private Collider _hoverCollider;

        public override void Register(Entity entity)
        {
            entity.AddEssenceHoverCollider(_hoverCollider);
        }
    }
}
