using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.MovementFeature
{
    [RequireComponent(typeof(Animator))]
    public class WalkingView : EntityView
    {
        private const int DEFAULT_LOCOMOTION_LAYER_INDEX = 0;

        private readonly int IsWalkingKey = Animator.StringToHash("IsWalking");

        [SerializeField] private Animator _animator;
        [SerializeField] private int _locomotionLayerIndex = DEFAULT_LOCOMOTION_LAYER_INDEX;

        private IReadOnlyVariable<bool> _isMoving;
        private IDisposable _isMovingChangedDisposable;

        private void OnValidate()
        {
            _animator ??= GetComponent<Animator>();
        }

        protected override void OnEntityStartedWork(Entity entity)
        {
            _isMoving = entity.IsMoving;
            _isMovingChangedDisposable = _isMoving.Subscribe(OnIsMovingChanged);

            _animator.SetLayerWeight(_locomotionLayerIndex, 1f);
            UpdateWalkAnimator();
        }

        public override void Cleanup(Entity entity)
        {
            base.Cleanup(entity);
            _isMovingChangedDisposable?.Dispose();
        }

        private void OnIsMovingChanged(bool oldIsMoving, bool isMoving) => UpdateWalkAnimator();

        private void UpdateWalkAnimator()
        {
            _animator.SetBool(IsWalkingKey, _isMoving.Value);
        }
    }
}
