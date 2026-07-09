using Assets._Project.Develop.Runtime.Configs.Gameplay.Gnome;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using Assets._Project.Develop.Runtime.Utilities.StateMachineCore;
using Random = UnityEngine.Random;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.AI.States
{
    public class GnomeHiddenState : State, IUpdatableState
    {
        private readonly GnomeConfig _config;
        private readonly ReactiveVariable<bool> _isPeeking;
        private readonly ReactiveVariable<bool> _forcePeekRequested;

        private float _elapsedSeconds;
        private float _hiddenDurationSeconds;

        private bool _isActivated;

        public bool ShouldPeek { get; private set; }

        public GnomeHiddenState(Entity entity, GnomeConfig config)
        {
            _config = config;
            _isPeeking = entity.IsPeeking;
            _forcePeekRequested = entity.GnomeForcePeekRequested;
        }

        public override void Enter()
        {
            base.Enter();

            _isPeeking.Value = false;
            _forcePeekRequested.Value = false;
            _elapsedSeconds = 0f;
            _hiddenDurationSeconds = Random.Range(_config.HiddenDurationMinSeconds, _config.HiddenDurationMaxSeconds);
            ShouldPeek = false;
        }

        public void Update(float deltaTime)
        {
            if (ShouldPeek == true)
                return;

            if (_forcePeekRequested.Value == true)
            {
                _isActivated = true;
                ShouldPeek = true;
                return;
            }

            if (_isActivated == false)
                return;

            _elapsedSeconds += deltaTime;

            if (_elapsedSeconds >= _hiddenDurationSeconds)
                ShouldPeek = true;
        }
    }
}
