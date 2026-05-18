using System;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Utilities.Reactive;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.DistanceDetector
{
    public class DetonationProcessTimerSystem : IInitializableSystem, IDisposableSystem, IUpdatableSystem
    {
        private ReactiveVariable<float> _currentTime;

        private ReactiveVariable<bool> _inDetonateProcess;

        private ReactiveEvent _startTauntEvent;

        private IDisposable _detonateRequestDispose;

        public void OnInit(Entity entity)
        {
            _currentTime = entity.DetonateProcessCurrentTime;
            _inDetonateProcess = entity.InDetonateProcess;
            
            _startTauntEvent = entity.StartTauntEvent;

            _detonateRequestDispose = _startTauntEvent.Subscribe(OnTauntStarted);
        }

        private void OnTauntStarted()
        {
            _currentTime.Value = 0;
        }

        public void OnUpdate(float deltaTime)
        {
            if (_inDetonateProcess.Value == false)
                return;

            _currentTime.Value += deltaTime;
        }

        public void OnDispose()
        {
            _detonateRequestDispose.Dispose();
        }
    }
}