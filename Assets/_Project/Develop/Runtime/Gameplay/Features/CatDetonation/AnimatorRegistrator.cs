using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.DistanceDetector
{
    public class AnimatorRegistrator : MonoEntityRegistrator
    {
        [SerializeField] private Animator _animator;

        public override void Register(Entity entity)
        {
            entity.AddAnimator(_animator);
        }
    }
}