using System;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.DistanceDetector
{
    public class ExplosionDelayEndTriggerSystem : IInitializableSystem, IDisposableSystem
    {
        private ReactiveEvent _hideExplosionSourceEvent;
        private ReactiveEvent _startExplosionEvent;
        private ReactiveVariable<float> _delay;
        private ReactiveVariable<float> _explosionCurrentTime;
        private ReactiveVariable<bool> _inDetonateProcess;
        private ReactiveVariable<bool> _inExplosionProcess;
        private Entity _entity;
        
        private bool _alreadyHidden;

        private IDisposable _timerDisposable;
        private IDisposable _startExplosionDisposable;

        public void OnInit(Entity entity)
        {
            _entity  = entity;

            _inExplosionProcess = entity.InExplosionProcess;
            _inDetonateProcess = entity.InDetonateProcess;
            _hideExplosionSourceEvent = entity.HideExplosionSourceEvent;
            _delay = entity.HideExplosionSourceDelayTime;
            _explosionCurrentTime = entity.ExplosionCurrentTime;
            _startExplosionEvent = entity.StartExplosionEvent;

            _timerDisposable = _explosionCurrentTime.Subscribe(OnTimerChanged);
            _startExplosionDisposable = _startExplosionEvent.Subscribe(OnStartExplosion);
        }

        private void OnStartExplosion()
        {
            _alreadyHidden = false;
        }

        private void OnTimerChanged(float arg1, float currentTime)
        {
            if (_alreadyHidden)
                return;

            if(currentTime >= _delay.Value)
            {
                Debug.Log("Задержка перед спрятыванием закончилась");

                _entity.IsDead.Value = true;
                _inExplosionProcess.Value = false;
                _inDetonateProcess.Value = false;
                _alreadyHidden = true;
                
                _hideExplosionSourceEvent.Invoke();
            }
        }

        public void OnDispose()
        {
            _timerDisposable.Dispose();
            _startExplosionDisposable.Dispose();
        }
    }
}