using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.EssenceFeature
{
    public class EssenceHoverUnlockSystem : IInitializableSystem, IUpdatableSystem
    {
        private ReactiveVariable<float> _remainingHoverUnlockTime;
        private ReactiveVariable<bool> _canAcceptHover;
        private ReactiveEvent _hoverReadyEvent;
        private Collider _hoverCollider;

        public void OnInit(Entity entity)
        {
            _remainingHoverUnlockTime = entity.EssenceHoverUnlockRemainingTime;
            _canAcceptHover = entity.EssenceCanAcceptHover;
            _hoverReadyEvent = entity.EssenceHoverReadyEvent;
            _hoverCollider = entity.EssenceHoverCollider;
        }

        public void OnUpdate(float deltaTime)
        {
            if (_canAcceptHover.Value)
                return;

            _remainingHoverUnlockTime.Value -= deltaTime;

            if (_remainingHoverUnlockTime.Value > 0f)
                return;

            ActivateHoverReady();
        }

        private void ActivateHoverReady()
        {
            if (_canAcceptHover.Value)
                return;

            _remainingHoverUnlockTime.Value = 0f;
            _canAcceptHover.Value = true;

            if (_hoverCollider != null)
                _hoverCollider.enabled = true;

            _hoverReadyEvent.Invoke();
        }
    }
}
