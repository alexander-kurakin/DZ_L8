using System;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using Assets._Project.Develop.Runtime.Utilities.Audio;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.DistanceDetector
{
    [RequireComponent(typeof(Animator))]
    public class DragonBeamDistanceReachedView : EntityView
    {
        private readonly int IsTargetReachedKey = Animator.StringToHash("IsTargetReached");

        [SerializeField] private Animator _animator;
        
        [SerializeField] private ParticleSystem _distanceReachedBeamPrefab;
        [SerializeField] private Transform _beamSource;
        [SerializeField] private AudioSource _localAudioSource;

        private ReactiveEvent _distanceReachedEvent;
        private ReactiveVariable<Entity> _currentTarget;
        private ReactiveVariable<bool> _ownerIsDead;
        private ParticleSystem _beamInstance;
        
        private IDisposable _distanceReachedDisposable;
        private IDisposable _currentTargetChangedDisposable;
       
        private IDisposable _ownerIsDeadDisposable;
        private IDisposable _targetIsDeadDisposable;
        
        private void OnValidate()
        {
            _animator ??= GetComponent<Animator>();
        }

        protected override void OnEntityStartedWork(Entity entity)
        {
            _distanceReachedEvent = entity.DistanceToTargetReachedEvent;
            _currentTarget = entity.CurrentTarget;
            _ownerIsDead = entity.IsDead;
            
            _currentTargetChangedDisposable = _currentTarget.Subscribe(OnCurrentTargetChanged);
            _distanceReachedDisposable = _distanceReachedEvent.Subscribe(OnDistanceReached);
            _ownerIsDeadDisposable = _ownerIsDead.Subscribe(OnOwnerIsDeadChanged);
        }

        private void OnDistanceReached()
        {
            _animator.SetBool(IsTargetReachedKey, true);
            
            if (_currentTarget.Value == null || _beamInstance != null)
                return;
            
            Vector3 direction = _currentTarget.Value.Transform.position - _beamSource.position;
            
            Quaternion rotation = direction.sqrMagnitude < 0.05f
                ? Quaternion.identity
                : Quaternion.LookRotation(direction.normalized);
            
            _beamInstance = Instantiate(
                _distanceReachedBeamPrefab,
                _beamSource.position,
                rotation);
            
            GameSoundsService.Play(GameSoundsIDs.DragonBurn, _localAudioSource);
        }
        
        private void OnCurrentTargetChanged(Entity oldTarget, Entity newTarget)
        {
            _targetIsDeadDisposable?.Dispose();
            
            _targetIsDeadDisposable = newTarget.IsDead.Subscribe(OnTargetIsDeadChanged);     
        }

        private void OnTargetIsDeadChanged(bool oldDead, bool newDead)
        {
            if (newDead)
                StopBeam();
        }

        private void OnOwnerIsDeadChanged(bool oldDead, bool newDead)
        {
            if (newDead)
                StopBeam();       
        }

        private void StopBeam()
        {
            _localAudioSource.Stop();
            
            if (_beamInstance == null)
                return;
            
            ParticleSystem beam = _beamInstance;
            
            _beamInstance = null;
            beam.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            
            Destroy(beam.gameObject);
        }

        public override void Cleanup(Entity entity)
        {
            base.Cleanup(entity);

            _distanceReachedDisposable?.Dispose();
            _currentTargetChangedDisposable?.Dispose();
            _ownerIsDeadDisposable?.Dispose();
            _targetIsDeadDisposable?.Dispose();
        }
    }
}