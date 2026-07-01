using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System;
using Assets._Project.Develop.Runtime.Gameplay.Features.GameplayStateBridge;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.MovementFeature
{
    [RequireComponent(typeof(Animator))]
    public class WalkingView : EntityView
    {
        private readonly int IsMovingKey = Animator.StringToHash("IsWalking");

        [SerializeField] private Animator _animator;

        private IReadOnlyVariable<bool> _isMoving;

        private bool _hasIsCurrentlyIdle;
        private IReadOnlyVariable<bool> _isCurrentlyIdle;
        
        private bool _hasGameplayPhase;
        private ReactiveVariable<GameplayStates> _gameplayPhase;

        private IDisposable _isMovingChangedDisposable;
        private IDisposable _isCurrentlyIdleChangedDisposable;
        private IDisposable _gameplayPhaseChangedDisposable;

        private void OnValidate()
        {
            _animator ??= GetComponent<Animator>();
        }

        protected override void OnEntityStartedWork(Entity entity)
        {
            _isMoving = entity.IsMoving;
            _hasIsCurrentlyIdle = entity.TryGetIsCurrentlyIdle(out ReactiveVariable<bool> isCurrentlyIdle);

            if (_hasIsCurrentlyIdle)
            {
                _isCurrentlyIdle = isCurrentlyIdle;
                _isCurrentlyIdleChangedDisposable = isCurrentlyIdle.Subscribe(OnIsCurrentlyIdleChanged);
            }

            _hasGameplayPhase = entity.TryGetGameplayPhase(out _gameplayPhase);

            if (_hasGameplayPhase)
                _gameplayPhaseChangedDisposable = _gameplayPhase.Subscribe(OnGameplayPhaseChanged);

            _isMovingChangedDisposable = _isMoving.Subscribe(OnIsMovingChanged);

            UpdateWalkAnimator();
        }

        public override void Cleanup(Entity entity)
        {
            base.Cleanup(entity);

            _isMovingChangedDisposable?.Dispose();
            _isCurrentlyIdleChangedDisposable?.Dispose();
            _gameplayPhaseChangedDisposable?.Dispose();
        }

        private void OnIsMovingChanged(bool oldIsMoving, bool isMoving) => UpdateWalkAnimator();

        private void OnIsCurrentlyIdleChanged(bool oldIsCurrentlyIdle, bool isCurrentlyIdle) => UpdateWalkAnimator();
        
        private void OnGameplayPhaseChanged(GameplayStates arg1, GameplayStates arg2) => UpdateWalkAnimator();

        private void UpdateWalkAnimator()
        {
            bool isWalkablePhase = _hasGameplayPhase == false || _gameplayPhase.Value == GameplayStates.StageProcess;
            bool shouldPlayWalkAnimation = ResolveShouldPlayWalkAnimation(isWalkablePhase);

            _animator.SetBool(IsMovingKey, shouldPlayWalkAnimation);
        }

        private bool ResolveShouldPlayWalkAnimation(bool isWalkablePhase)
        {
            if (isWalkablePhase == false)
                return false;

            if (_hasIsCurrentlyIdle)
                return _isCurrentlyIdle.Value == false;

            return _isMoving.Value;
        }
    }
}
