using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Utilities.Conditions;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.TowerWalker
{
    public class BrotherStoneArcMovementSystem : IInitializableSystem, IUpdatableSystem
    {
        private const float MIN_MOVEMENT_DELTA_SQR_MAGNITUDE = 0.0001f;

        private readonly TowerBrotherStoneThrowConfig _config;

        private Entity _entity;
        private BrotherStoneArcFlight _arcFlight;
        private Rigidbody _rigidbody;
        private SphereCollider _sphereCollider;
        private ReactiveVariable<Vector3> _moveDirection;
        private ReactiveVariable<Vector3> _rotationDirection;
        private ReactiveVariable<bool> _isMoving;
        private ReactiveVariable<bool> _isDead;
        private ReactiveVariable<bool> _isTouchAnotherTeam;
        private ReactiveVariable<float> _contactDamage;
        private ICompositeCondition _canMove;

        private Vector3 _previousPosition;

        public BrotherStoneArcMovementSystem(TowerBrotherStoneThrowConfig config)
        {
            _config = config;
        }

        public void OnInit(Entity entity)
        {
            _entity = entity;
            entity.TryGetComponent(out _arcFlight);
            _rigidbody = entity.Rigidbody;
            entity.TryGetMineCollider(out _sphereCollider);
            _moveDirection = entity.MoveDirection;
            _rotationDirection = entity.RotationDirection;
            _isMoving = entity.IsMoving;
            _isDead = entity.IsDead;
            _isTouchAnotherTeam = entity.IsTouchAnotherTeam;
            _contactDamage = entity.BodyContactDamage;
            _canMove = entity.CanMove;

            _rigidbody.isKinematic = true;

            if (_sphereCollider != null)
                _sphereCollider.isTrigger = true;

            _previousPosition = _arcFlight != null ? _arcFlight.StartPosition : _rigidbody.position;
        }

        public void OnUpdate(float deltaTime)
        {
            if (_arcFlight == null)
                return;

            if (_canMove.Evaluate() == false || _isDead.Value || _arcFlight.IsCompleted)
            {
                _isMoving.Value = false;
                return;
            }

            if (_arcFlight.Speed <= 0f || _arcFlight.TotalDistance <= 0f)
            {
                CompleteFlight();
                return;
            }

            float flightDuration = _arcFlight.TotalDistance / _arcFlight.Speed;
            _arcFlight.TraveledTime += deltaTime;
            float normalizedProgress = Mathf.Clamp01(_arcFlight.TraveledTime / flightDuration);

            Vector3 newPosition = BrotherStoneArcUtility.EvaluateArcPosition(
                _arcFlight.StartPosition,
                _arcFlight.TargetPosition,
                normalizedProgress,
                _config.ArcHeightCurve,
                _config.ArcMaxHeight);

            _rigidbody.MovePosition(newPosition);

            Vector3 movementDelta = newPosition - _previousPosition;

            if (movementDelta.sqrMagnitude > MIN_MOVEMENT_DELTA_SQR_MAGNITUDE)
            {
                Vector3 flyDirection = movementDelta.normalized;
                _moveDirection.Value = flyDirection;
                _rotationDirection.Value = flyDirection;
                _isMoving.Value = true;
            }
            else
            {
                _isMoving.Value = false;
            }

            _previousPosition = newPosition;

            if (normalizedProgress >= 1f)
                CompleteFlight();
        }

        private void CompleteFlight()
        {
            if (_arcFlight.IsCompleted)
                return;

            _arcFlight.IsCompleted = true;
            _isMoving.Value = false;

            if (_isTouchAnotherTeam.Value == false && _arcFlight.TargetEntity != null)
            {
                bool damageApplied = EntitiesHelper.TryTakeDamageFrom(
                    _entity,
                    _arcFlight.TargetEntity,
                    _contactDamage.Value);

                if (damageApplied)
                    _isTouchAnotherTeam.Value = true;
            }

            if (_isTouchAnotherTeam.Value == false)
                _isDead.Value = true;
        }
    }
}
