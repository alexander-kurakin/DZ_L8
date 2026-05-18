using System;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.DistanceDetector
{
    public class StartExplosionSystem : IInitializableSystem, IDisposableSystem
    {
        private ReactiveEvent _tauntFinishedEvent;
        private ReactiveEvent _startExplosionEvent;
        private ReactiveVariable<bool> _inExplosionProcess;

        private IDisposable _tauntFinishedEventDispose;

        public void OnInit(Entity entity)
        {
            _tauntFinishedEvent = entity.TauntFinishedEvent;
            
            _startExplosionEvent = entity.StartExplosionEvent;
            _inExplosionProcess = entity.InExplosionProcess; 

            _tauntFinishedEventDispose = _tauntFinishedEvent.Subscribe(OnTauntFinished);
        }

        private void OnTauntFinished()
        {
            _inExplosionProcess.Value = true;
            _startExplosionEvent.Invoke();

            Debug.Log("Старт взрыв стадии");
        }

        public void OnDispose()
        {
            _tauntFinishedEventDispose.Dispose();
        }
    } 
}