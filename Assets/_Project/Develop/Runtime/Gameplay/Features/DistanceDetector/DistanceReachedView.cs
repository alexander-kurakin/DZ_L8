using System;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using Assets._Project.Develop.Runtime.Utilities.Audio;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.DistanceDetector
{
    public class DistanceReachedView : EntityView
    {
        [SerializeField] private ParticleSystem _distanceReachedBeamPrefab;
        [SerializeField] private Transform _beamSource;
        [SerializeField] private AudioSource _localAudioSource;

        private ReactiveEvent _isDistanceReached;
        private Transform _currentTargetTransform;
        
        private IDisposable _isDistanceReachedDisposable;
        private IDisposable _currentTargetChangedDisposable;
        private IDisposable _isDeadDisposable;

        private bool _hasTargetToFireABeamAt;

        protected override void OnEntityStartedWork(Entity entity)
        {
            _isDistanceReached = entity.DistanceToTargetReachedEvent;
            
            _currentTargetChangedDisposable = entity.CurrentTarget.Subscribe(OnCurrentTargetChanged);
            _isDistanceReachedDisposable = _isDistanceReached.Subscribe(OnDistanceReached);
            _isDeadDisposable = entity.IsDead.Subscribe(OnIsDeadChanged);
        }

        private void OnIsDeadChanged(bool oldDead, bool newDead)
        {
            if (newDead)
                _localAudioSource.Stop();
        }

        private void OnCurrentTargetChanged(Entity oldTarget, Entity newTarget)
        {
            if (newTarget != null)
            {
                _currentTargetTransform = newTarget.Transform;
                _hasTargetToFireABeamAt = true;
            }
        }

        private void OnDistanceReached()
        {
            if (_hasTargetToFireABeamAt)
            {
                GameSoundsService.Play(GameSoundsIDs.DragonBurn, _localAudioSource);
                
                Quaternion targetRotation;
                Vector3 targetDirection = _currentTargetTransform.position - _beamSource.position;

                if (targetDirection.sqrMagnitude < 0.05f)
                {
                    targetRotation = Quaternion.identity;
                }
                else
                {
                    Vector3 normalizedDirection = targetDirection.normalized;
                    targetRotation = Quaternion.LookRotation(normalizedDirection);
                }

                Instantiate(
                    _distanceReachedBeamPrefab,
                    _beamSource.position,
                    targetRotation);
            }
        }

        public override void Cleanup(Entity entity)
        {
            base.Cleanup(entity);

            _isDistanceReachedDisposable.Dispose();
            _currentTargetChangedDisposable.Dispose();
            _isDeadDisposable.Dispose();
        }
    }
}