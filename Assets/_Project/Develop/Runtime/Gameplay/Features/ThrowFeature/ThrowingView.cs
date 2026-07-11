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
        private const int DEFAULT_UPPER_BODY_THROW_LAYER_INDEX = 1;

        private readonly int IsCharging = Animator.StringToHash("IsCharging");

        [SerializeField] private Animator _animator;
        [SerializeField] private int _upperBodyThrowLayerIndex = DEFAULT_UPPER_BODY_THROW_LAYER_INDEX;

        private IReadOnlyVariable<bool> _isCharging;
        private IReadOnlyVariable<bool> _isProjectileInHand;
        private IReadOnlyVariable<bool> _isWatchingThrownProjectile;
        private IDisposable _isChargingChangedDisposable;
        private IDisposable _isProjectileInHandChangedDisposable;
        private IDisposable _isWatchingThrownProjectileChangedDisposable;

        private void OnValidate()
        {
            _animator ??= GetComponent<Animator>();
        }

        protected override void OnEntityStartedWork(Entity entity)
        {
            _isCharging = entity.IsChargingThrow;
            _isProjectileInHand = entity.IsProjectileInHand;
            _isWatchingThrownProjectile = entity.IsWatchingThrownProjectile;
            _isChargingChangedDisposable = _isCharging.Subscribe(OnThrowAnimationStateChanged);
            _isProjectileInHandChangedDisposable = _isProjectileInHand.Subscribe(OnThrowAnimationStateChanged);
            _isWatchingThrownProjectileChangedDisposable = _isWatchingThrownProjectile.Subscribe(OnThrowAnimationStateChanged);

            UpdateAnimator();
        }

        public override void Cleanup(Entity entity)
        {
            base.Cleanup(entity);
            _isChargingChangedDisposable?.Dispose();
            _isProjectileInHandChangedDisposable?.Dispose();
            _isWatchingThrownProjectileChangedDisposable?.Dispose();
        }

        private void OnThrowAnimationStateChanged(bool oldValue, bool newValue) => UpdateAnimator();

        private void UpdateAnimator()
        {
            bool isUpperBodyThrowActive = _isProjectileInHand.Value == true
                || _isCharging.Value == true
                || _isWatchingThrownProjectile.Value == true;
            float upperBodyLayerWeight = isUpperBodyThrowActive ? 1f : 0f;

            _animator.SetLayerWeight(_upperBodyThrowLayerIndex, upperBodyLayerWeight);
            _animator.SetBool(IsCharging, _isCharging.Value);
        }
    }
}
