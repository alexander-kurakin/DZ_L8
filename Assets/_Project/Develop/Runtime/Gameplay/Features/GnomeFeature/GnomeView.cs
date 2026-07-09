using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using DG.Tweening;
using System;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.GnomeFeature
{
    public class GnomeView : EntityView
    {
        private const float PEEK_TWEEN_DURATION_SECONDS = 0.2f;
        private const float VERTICAL_VISUAL_ROTATION_X = 90f;

        [SerializeField] private Transform _visualRoot;
        [SerializeField] private Collider _hitCollider;

        private IReadOnlyVariable<bool> _isPeeking;
        private Transform _entityTransform;
        private Vector3 _peekDirection;
        private float _peekOffset;
        private Vector3 _hiddenLocalPosition;
        private Tween _peekTween;
        private IDisposable _isPeekingSubscription;

        protected override void OnEntityStartedWork(Entity entity)
        {
            if (entity.TryGetTransform(out Transform entityTransform) == false)
                throw new InvalidOperationException("Gnome entity transform is missing.");

            _entityTransform = entityTransform;
            _isPeeking = entity.IsPeeking;
            _peekDirection = entity.GnomePeekDirection;
            _peekOffset = entity.GnomePeekOffset;

            if (entity.TryGetComponent(out GnomeIsVerticalLayout verticalLayout) == true && verticalLayout.Value == true)
                _visualRoot.localRotation = Quaternion.Euler(VERTICAL_VISUAL_ROTATION_X, 0f, 0f);

            _hiddenLocalPosition = _visualRoot.localPosition;

            _isPeekingSubscription = _isPeeking.Subscribe(OnPeekingChanged);

            if (_isPeeking.Value == true)
                ShowPeeked();
            else
                ShowHidden();
        }

        public override void Cleanup(Entity entity)
        {
            base.Cleanup(entity);

            _peekTween?.Kill();
            _isPeekingSubscription?.Dispose();
        }

        private void OnPeekingChanged(bool oldValue, bool isPeeking)
        {
            if (isPeeking == true)
                ShowPeeked();
            else
                ShowHidden();
        }

        private void ShowHidden()
        {
            _peekTween?.Kill();

            if (_hitCollider != null)
                _hitCollider.enabled = false;

            _visualRoot.localPosition = _hiddenLocalPosition;
        }

        private void ShowPeeked()
        {
            _peekTween?.Kill();

            if (_hitCollider != null)
                _hitCollider.enabled = true;

            Vector3 hiddenWorldPosition = _entityTransform.TransformPoint(_hiddenLocalPosition);
            Vector3 peekWorldPosition = hiddenWorldPosition + _peekDirection * _peekOffset;

            _peekTween = _visualRoot
                .DOMove(peekWorldPosition, PEEK_TWEEN_DURATION_SECONDS)
                .SetEase(Ease.OutCubic)
                .SetUpdate(true)
                .Play();
        }
    }
}
