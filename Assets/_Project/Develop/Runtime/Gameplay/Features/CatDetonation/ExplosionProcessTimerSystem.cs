using System;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Utilities.Reactive;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.DistanceDetector
{
    public class ExplosionProcessTimerSystem : IInitializableSystem, IDisposableSystem, IUpdatableSystem
    {
        private ReactiveVariable<float> _currentTime;

        private ReactiveVariable<bool> _inExplosionProcess;

        private ReactiveEvent _startExplosionEvent;

        private IDisposable _startExplosionEventDispose;

        public void OnInit(Entity entity)
        {
            _currentTime = entity.ExplosionCurrentTime;
            _inExplosionProcess = entity.InExplosionProcess;

            _startExplosionEvent = entity.StartExplosionEvent;

            _startExplosionEventDispose = _startExplosionEvent.Subscribe(OnExplosionStarted);
        }

        private void OnExplosionStarted()
        {
            _currentTime.Value = 0;
        }

        public void OnUpdate(float deltaTime)
        {
            if (_inExplosionProcess.Value == false)
                return;

            _currentTime.Value += deltaTime;
        }

        public void OnDispose()
        {
            _startExplosionEventDispose.Dispose();
        }
    }
}