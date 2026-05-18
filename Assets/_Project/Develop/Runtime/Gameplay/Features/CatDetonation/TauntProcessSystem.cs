using System;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.DistanceDetector
{
    public class TauntProcessSystem : IInitializableSystem, IDisposableSystem, IUpdatableSystem
    {
        private static readonly int AllDoneHash = Animator.StringToHash("ALL DONE");
        
        private ReactiveVariable<bool> _inDetonateProcess;

        private ReactiveEvent _startTauntEvent;
        private ReactiveEvent _endTauntEvent;
        
        private Animator _animator;

        private IDisposable _startTauntEventDisposable;

        private bool _isTrackingTaunt;
        
        public void OnInit(Entity entity)
        {
            _inDetonateProcess = entity.InDetonateProcess;
            _startTauntEvent = entity.StartTauntEvent;
            _endTauntEvent = entity.TauntFinishedEvent;
            _animator = entity.Animator;

            _startTauntEventDisposable = _startTauntEvent.Subscribe(OnStartTauntProcess);
        }

        private void OnStartTauntProcess()
        {
            _isTrackingTaunt = true;
        }

        public void OnUpdate(float deltaTime)
        {
            if (_isTrackingTaunt == false)
                return;
            
            if (_inDetonateProcess.Value == false)
                return;
            
            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);

            if (stateInfo.shortNameHash == AllDoneHash)
            {
                _isTrackingTaunt = false;
                _endTauntEvent.Invoke();
            }
        }

        public void OnDispose()
        {
            _startTauntEventDisposable.Dispose();
        }
    }
}