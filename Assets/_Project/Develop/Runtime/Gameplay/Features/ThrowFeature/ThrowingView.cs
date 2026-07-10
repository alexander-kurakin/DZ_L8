using System;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.ThrowFeature
{
    [RequireComponent(typeof(Animator))]
    public class ThrowingView : EntityView
    {
        private readonly int IsCharging = Animator.StringToHash("IsCharging");

        [SerializeField] private Animator _animator;

        private IReadOnlyVariable<bool> _isCharging;
        private IDisposable _isChargingChangedDisposable;

        private void OnValidate()
        {
            _animator ??= GetComponent<Animator>();
        }

        protected override void OnEntityStartedWork(Entity entity)
        {
            _isCharging = entity.IsChargingThrow;
            _isChargingChangedDisposable = _isCharging.Subscribe(OnIsChargingChanged);

            UpdateAnimator();
        }

        public override void Cleanup(Entity entity)
        {
            base.Cleanup(entity);
            _isChargingChangedDisposable?.Dispose();
        }

        private void OnIsChargingChanged(bool oldIsCharging, bool isCharging) => UpdateAnimator();

        private void UpdateAnimator()
        {
            _animator.SetBool(IsCharging, _isCharging.Value);
        }
    }
}