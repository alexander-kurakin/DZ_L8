using System;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Utilities.Conditions;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.DistanceDetector
{
    public class StartDetonationSequence : IInitializableSystem, IDisposableSystem
    {
        private const int MaxTauntsVariants = 6;
        
        private ReactiveEvent _distanceReachedRequest;
        private ReactiveEvent _startTauntEvent;
        private ReactiveVariable<bool> _inDetonationProcess;
        private ReactiveVariable<int> _detonateTauntIndex;
        private ICompositeCondition _canStartDetonate;

        private IDisposable _detonateRequestDispose;

        public void OnInit(Entity entity)
        {
            _distanceReachedRequest = entity.DistanceToTargetReachedEvent;
            _startTauntEvent = entity.StartTauntEvent;
            _inDetonationProcess = entity.InDetonateProcess; 
            _canStartDetonate = entity.CanStartDetonate;
            _detonateTauntIndex = entity.DetonateTauntIndex;

            _detonateRequestDispose = _distanceReachedRequest.Subscribe(OnDistanceReached);
        }

        private void OnDistanceReached()
        {
            if (_canStartDetonate.Evaluate())
            {
                _inDetonationProcess.Value = true;
                
                _detonateTauntIndex.Value = Random.Range(1, MaxTauntsVariants + 1);
                
                _startTauntEvent.Invoke();
                Debug.Log("Старт таунт стадии");
            }
            else
            {
                Debug.Log("Не могу детонировать!");
            }
        }

        public void OnDispose()
        {
            _detonateRequestDispose.Dispose();
        }
    } 
}