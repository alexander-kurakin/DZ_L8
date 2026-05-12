using System;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using Assets._Project.Develop.Runtime.Utilities.Audio;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.TowerWalker
{
    [RequireComponent(typeof(Animator))]
    public class MagicCastingView : EntityView
    {
        private readonly int MagicCastedHash = Animator.StringToHash("MagicCasted");

        [SerializeField] private Animator _animator;
        [SerializeField] private ParticleSystem _castVfxPrefab;
        
        [SerializeField] private GameSoundsIDs _castSound;
        [SerializeField] private AudioSource _localAudioSource;

        private IDisposable _magicCastRequest;

        private void OnValidate()
        {
            _animator ??= GetComponent<Animator>();
        }

        protected override void OnEntityStartedWork(Entity entity)
        {
            _magicCastRequest = entity.MagicCastRequestedEvent.Subscribe(OnMagicCastRequested);
        }

        public override void Cleanup(Entity entity)
        {
            base.Cleanup(entity);

            _magicCastRequest?.Dispose();
        }

        private void OnMagicCastRequested(Vector3 worldPoint)
        {
            _animator.SetTrigger(MagicCastedHash);

            Instantiate(_castVfxPrefab, worldPoint, Quaternion.identity);
            
            GameSoundsService.PlayOneShot(_castSound, _localAudioSource);
        }
    }
}