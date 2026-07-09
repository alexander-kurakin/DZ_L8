using Assets._Project.Develop.Runtime.Configs.Gameplay.Gnome;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.GnomeFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.MainHero;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using Assets._Project.Develop.Runtime.Utilities.StateMachineCore;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.AI.States
{
    public class GnomePeekingState : State, IUpdatableState
    {
        private readonly Entity _entity;
        private readonly GnomeConfig _config;
        private readonly MainHeroHolderService _mainHeroHolderService;
        private readonly ReactiveVariable<bool> _isPeeking;
        private readonly ReactiveVariable<Vector3> _rotationDirection;

        private float _elapsedSeconds;
        private float _peekDurationSeconds;

        public bool ShouldHide { get; private set; }

        public GnomePeekingState(Entity entity, GnomeConfig config, MainHeroHolderService mainHeroHolderService)
        {
            _entity = entity;
            _config = config;
            _mainHeroHolderService = mainHeroHolderService;
            _isPeeking = entity.IsPeeking;
            _rotationDirection = entity.RotationDirection;
        }

        public override void Enter()
        {
            base.Enter();

            _isPeeking.Value = true;
            _elapsedSeconds = 0f;
            _peekDurationSeconds = Random.Range(_config.PeekDurationMinSeconds, _config.PeekDurationMaxSeconds);
            ShouldHide = false;
        }

        public override void Exit()
        {
            base.Exit();

            _isPeeking.Value = false;
            _rotationDirection.Value = Vector3.zero;
        }

        public void Update(float deltaTime)
        {
            if (ShouldHide == true)
                return;

            RotateTowardHero();

            _elapsedSeconds += deltaTime;

            if (_elapsedSeconds >= _peekDurationSeconds)
                ShouldHide = true;
        }

        private void RotateTowardHero()
        {
            if (_entity.TryGetComponent(out GnomeIsVerticalLayout verticalLayout) == true && verticalLayout.Value == true)
                return;

            Entity hero = _mainHeroHolderService.MainHero;

            if (hero == null)
                return;

            if (hero.TryGetTransform(out Transform heroTransform) == false)
                return;

            if (_entity.TryGetTransform(out Transform gnomeTransform) == false)
                return;

            Vector3 direction = heroTransform.position - gnomeTransform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude <= 0.0001f)
                return;

            _rotationDirection.Value = direction.normalized;
        }
    }
}
