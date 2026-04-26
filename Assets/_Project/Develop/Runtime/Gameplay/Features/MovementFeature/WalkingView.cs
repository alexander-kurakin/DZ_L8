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
        
        private bool _hasGameplayPhase;
        private ReactiveVariable<GameplayStates> _gameplayPhase;

        private IDisposable _isMovingChangedDisposable;
        private IDisposable _gameplayPhaseChangedDisposable;

        private void OnValidate()
        {
            _animator ??= GetComponent<Animator>();
        }

        protected override void OnEntityStartedWork(Entity entity)
        {
            _isMoving = entity.IsMoving;
            
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
            _gameplayPhaseChangedDisposable?.Dispose();
        }

        private void OnIsMovingChanged(bool oldIsMoving, bool isMoving) => UpdateWalkAnimator();
        
        private void OnGameplayPhaseChanged(GameplayStates arg1, GameplayStates arg2) => UpdateWalkAnimator();

        private void UpdateWalkAnimator()
        {
            bool isMoving = _isMoving.Value;
            
            bool isWalkablePhase = !_hasGameplayPhase || _gameplayPhase.Value == GameplayStates.StageProcess;
            
            _animator.SetBool(IsMovingKey, isMoving && isWalkablePhase);
        }
    }
}
