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

        [SerializeField] private Transform _visualRoot;

        private IReadOnlyVariable<bool> _isPeeking;
        private Transform _entityTransform;
        private Vector3 _peekDirection;
        private float _peekOffset;
        private float _peekLeanAngle;
        private Vector3 _hiddenLocalPosition;
        private Quaternion _hiddenLocalRotation;
        private Tween _peekTween;
        private IDisposable _isPeekingSubscription;

        protected override void OnEntityStartedWork(Entity entity)
        {
            if (entity.TryGetTransform(out Transform entityTransform) == false)
                throw new InvalidOperationException("Gnome entity transform is missing.");

            if (entity.TryGetComponent(out IsPeeking isPeeking) == false)
                throw new InvalidOperationException("Gnome IsPeeking component is missing.");

            if (entity.TryGetComponent(out GnomePeekDirection gnomePeekDirection) == false)
                throw new InvalidOperationException("Gnome GnomePeekDirection component is missing.");

            if (entity.TryGetComponent(out GnomePeekOffset gnomePeekOffset) == false)
                throw new InvalidOperationException("Gnome GnomePeekOffset component is missing.");

            _entityTransform = entityTransform;
            _isPeeking = isPeeking.Value;
            _peekDirection = gnomePeekDirection.Value;
            _peekOffset = gnomePeekOffset.Value;
            
            _peekLeanAngle = entity.TryGetComponent(out GnomePeekLeanAngle gnomePeekLeanAngle) == true
                ? gnomePeekLeanAngle.Value
                : 0f;
            
            _hiddenLocalPosition = _visualRoot.localPosition;
            _hiddenLocalRotation = _visualRoot.localRotation;

            _isPeekingSubscription = _isPeeking.Subscribe(OnPeekingChanged);

            if (_isPeeking.Value == true)
                ShowPeeked();
            else
                ShowHidden(animate: false);
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
                ShowHidden(animate: oldValue == true);
        }

        private void ShowHidden(bool animate)
        {
            _peekTween?.Kill();

            if (animate == false)
            {
                _visualRoot.localPosition = _hiddenLocalPosition;
                _visualRoot.localRotation = _hiddenLocalRotation;
                return;
            }
            
            Vector3 hiddenWorldPosition = _entityTransform.TransformPoint(_hiddenLocalPosition);

            if (_peekLeanAngle == 0f)
            {
                _peekTween = _visualRoot
                    .DOMove(hiddenWorldPosition, PEEK_TWEEN_DURATION_SECONDS)
                    .SetEase(Ease.OutCubic)
                    .SetUpdate(true)
                    .Play();

                return;
            }

            _peekTween = DOTween.Sequence()
                .Join(_visualRoot.DOMove(hiddenWorldPosition, PEEK_TWEEN_DURATION_SECONDS))
                .Join(_visualRoot.DOLocalRotateQuaternion(_hiddenLocalRotation, PEEK_TWEEN_DURATION_SECONDS))
                .SetEase(Ease.OutCubic)
                .SetUpdate(true)
                .Play();
        }

        private void ShowPeeked()
        {
            _peekTween?.Kill();

            Vector3 hiddenWorldPosition = _entityTransform.TransformPoint(_hiddenLocalPosition);
            Vector3 peekWorldPosition = hiddenWorldPosition + _peekDirection * _peekOffset;

            if (_peekLeanAngle == 0f)
            {
                _peekTween = _visualRoot
                    .DOMove(peekWorldPosition, PEEK_TWEEN_DURATION_SECONDS)
                    .SetEase(Ease.OutCubic)
                    .SetUpdate(true)
                    .Play();

                return;
            }

            Quaternion peekLocalRotation = GetPeekLocalRotation();

            _peekTween = DOTween.Sequence()
                .Join(_visualRoot.DOMove(peekWorldPosition, PEEK_TWEEN_DURATION_SECONDS))
                .Join(_visualRoot.DOLocalRotateQuaternion(peekLocalRotation, PEEK_TWEEN_DURATION_SECONDS))
                .SetEase(Ease.OutCubic)
                .SetUpdate(true)
                .Play();
        }

        private Quaternion GetPeekLocalRotation()
        {
            Transform rotationParent = _visualRoot.parent != null ? _visualRoot.parent : _entityTransform;
            Quaternion hiddenVisualWorldRotation = rotationParent.rotation * _hiddenLocalRotation;
            Quaternion peekVisualWorldRotation = GnomePeekPoint.GetPeekLeanRotation(
                hiddenVisualWorldRotation,
                _peekDirection,
                _peekLeanAngle);

            return Quaternion.Inverse(rotationParent.rotation) * peekVisualWorldRotation;
        }
    }
}
