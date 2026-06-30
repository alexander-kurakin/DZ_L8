using System;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.TowerWalker
{
    [RequireComponent(typeof(Animator))]
    public class BrotherIdleView : EntityView
    {
        private const int IdleVariantCount = 6;
        private static readonly int IdleVariantKey = Animator.StringToHash("IdleVariant");
        private static readonly int PlayIdleFidgetKey = Animator.StringToHash("PlayIdleFidget");
        
        [SerializeField] private Animator _animator;
        
        private ReactiveVariable<bool> _isCurrentlyIdle;
        private ReactiveVariable<bool> _isStoneThrowing;
        private IDisposable _isCurrentlyIdleDisposable;
        
        private void OnValidate() => _animator ??= GetComponent<Animator>();
        
        protected override void OnEntityStartedWork(Entity entity)
        {
            _isCurrentlyIdle = entity.IsCurrentlyIdle;
            _isStoneThrowing = entity.BrotherStoneThrowing;
            _isCurrentlyIdleDisposable = _isCurrentlyIdle.Subscribe(OnIsCurrentIdleChanged);
        }
        
        private void OnIsCurrentIdleChanged(bool prevIdle, bool newIdle)
        {
            if (newIdle == false || prevIdle)
                return;

            if (_isStoneThrowing.Value)
                return;
            
            _animator.SetInteger(IdleVariantKey, Random.Range(0, IdleVariantCount));
            _animator.SetTrigger(PlayIdleFidgetKey);
        }
        
        public override void Cleanup(Entity entity)
        {
            base.Cleanup(entity);
            
            _isCurrentlyIdleDisposable?.Dispose();
        }
    }
}