using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System;
using Assets._Project.Develop.Runtime.Utilities.Audio;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.LifeCycle
{
    [RequireComponent(typeof(Animator))]
    public class DeadView : EntityView
    {
        private readonly int IsDeadKey = Animator.StringToHash("IsDead");

        [SerializeField] private Animator _animator;
        
        [SerializeField] private GameSoundsIDs _deadSoundToPlay;
        [SerializeField] private AudioSource _localAudioSource;

        private ReactiveVariable<bool> _isDead;

        private IDisposable _isDeadChangedDisposable;

        private void OnValidate()
        {
            _animator ??= GetComponent<Animator>();
        }

        protected override void OnEntityStartedWork(Entity entity)
        {
            _isDead = entity.IsDead;

            _isDeadChangedDisposable = _isDead.Subscribe(OnIsDeadChanged);
            UpdateIsDead(_isDead.Value);
        }

        public override void Cleanup(Entity entity)
        {
            base.Cleanup(entity);

            _isDeadChangedDisposable.Dispose();
        }

        private void OnIsDeadChanged(bool oldIsDead, bool isDead) => UpdateIsDead(isDead);

        private void UpdateIsDead(bool value)
        {
            if (_animator != null)
                _animator.SetBool(IsDeadKey, value);
            
            if (value)
                GameSoundsService.PlayOneShot(_deadSoundToPlay, _localAudioSource);
        }
    }
}
