using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using Assets._Project.Develop.Runtime.Gameplay.Features.RunKillCounter;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using DG.Tweening;
using System;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.GnomeFeature
{
    public class GnomeDeathView : EntityView
    {
        [SerializeField] private Transform _visualRoot;

        private IReadOnlyVariable<bool> _isDead;
        private float _deathDissolveSeconds;
        private IDisposable _isDeadSubscription;
        private Tween _deathTween;
        private bool _isDeathStarted;

        protected override void OnEntityStartedWork(Entity entity)
        {
            if (entity.TryGetComponent(out RunEnemyKillMarker killMarker) == false)
                throw new InvalidOperationException("Gnome entity is missing RunEnemyKillMarker.");

            _isDead = killMarker.IsDead;
            _deathDissolveSeconds = entity.GnomeDeathDissolveDuration;

            _isDeadSubscription = _isDead.Subscribe(OnDeadChanged);
        }

        public override void Cleanup(Entity entity)
        {
            base.Cleanup(entity);

            _deathTween?.Kill();
            _isDeadSubscription?.Dispose();
        }

        private void OnDeadChanged(bool oldValue, bool isDead)
        {
            if (isDead == false)
                return;

            if (_isDeathStarted == true)
                return;

            _isDeathStarted = true;
            PlayDeathAnimation();
        }

        private void PlayDeathAnimation()
        {
            _deathTween?.Kill();

            _deathTween = _visualRoot
                .DOScale(Vector3.zero, _deathDissolveSeconds)
                .SetEase(Ease.InCubic)
                .SetUpdate(true)
                .Play();
        }
    }
}
