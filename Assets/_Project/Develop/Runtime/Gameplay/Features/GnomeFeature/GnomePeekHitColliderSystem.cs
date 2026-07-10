using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.GnomeFeature
{
    public class GnomePeekHitColliderSystem : IInitializableSystem, IUpdatableSystem
    {
        private ReactiveVariable<bool> _isPeeking;
        private CapsuleCollider _hitCollider;
        private bool _wasPeeking;

        public void OnInit(Entity entity)
        {
            _isPeeking = entity.IsPeeking;
            _hitCollider = entity.BodyCollider;
            _wasPeeking = _isPeeking.Value;
            _hitCollider.enabled = _wasPeeking;
        }

        public void OnUpdate(float deltaTime)
        {
            bool isPeeking = _isPeeking.Value;

            if (isPeeking == _wasPeeking)
                return;

            _wasPeeking = isPeeking;
            _hitCollider.enabled = isPeeking;
        }
    }
}
