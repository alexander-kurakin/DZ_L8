using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.TakeDamage
{
    public static class EnemyHitJuiceUtility
    {
        private const float MIN_KNOCKBACK_DIRECTION_SQR_MAGNITUDE = 0.0001f;
        private const float REFERENCE_DAMAGE = 50f;
        private const float PUNCH_DURATION_SECONDS = 0.16f;
        private const float MIN_SCALE_PUNCH = 1.08f;
        private const float MAX_SCALE_PUNCH = 1.16f;
        private const float MIN_HORIZONTAL_KNOCKBACK = 0.22f;
        private const float MAX_HORIZONTAL_KNOCKBACK = 0.5f;
        private const float MIN_VERTICAL_KNOCKBACK = 0.18f;
        private const float MAX_VERTICAL_KNOCKBACK = 0.4f;

        private static readonly Dictionary<Transform, Playback> ActivePlaybacks = new Dictionary<Transform, Playback>();

        public static void PlayOnTransform(Transform root, Vector3 spawnOrigin, float damage)
        {
            if (root == null)
                return;

            Transform visualTransform = ResolveVisualTransform(root);
            float damageFactor = Mathf.Clamp01(damage / REFERENCE_DAMAGE);

            float scaleMultiplier = Mathf.Lerp(MIN_SCALE_PUNCH, MAX_SCALE_PUNCH, damageFactor);
            float horizontalDistance = Mathf.Lerp(MIN_HORIZONTAL_KNOCKBACK, MAX_HORIZONTAL_KNOCKBACK, damageFactor);
            float verticalOffset = Mathf.Lerp(MIN_VERTICAL_KNOCKBACK, MAX_VERTICAL_KNOCKBACK, damageFactor);

            Vector3 punchOffset = Vector3.up * verticalOffset;

            Vector3 knockDirection = spawnOrigin - root.position;
            knockDirection.y = 0f;

            if (knockDirection.sqrMagnitude >= MIN_KNOCKBACK_DIRECTION_SQR_MAGNITUDE)
            {
                knockDirection.Normalize();
                punchOffset += knockDirection * horizontalDistance;
            }

            if (visualTransform.parent != null)
                punchOffset = visualTransform.parent.InverseTransformDirection(punchOffset);

            StopPlayback(visualTransform);

            Vector3 baseLocalScale = visualTransform.localScale;
            Vector3 targetScale = baseLocalScale * scaleMultiplier;

            Playback playback = new Playback
            {
                BaseLocalScale = baseLocalScale
            };

            playback.ScaleTween = visualTransform
                .DOScale(targetScale, PUNCH_DURATION_SECONDS * 0.45f)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    playback.ScaleTween = visualTransform
                        .DOScale(baseLocalScale, PUNCH_DURATION_SECONDS * 0.55f)
                        .SetEase(Ease.InQuad)
                        .SetUpdate(true)
                        .OnKill(() => FinishPlayback(visualTransform, playback))
                        .OnComplete(() => FinishPlayback(visualTransform, playback))
                        .Play();
                })
                .Play();

            playback.PositionTween = visualTransform
                .DOPunchPosition(punchOffset, PUNCH_DURATION_SECONDS, 1, 0.15f)
                .SetUpdate(true)
                .Play();

            ActivePlaybacks[visualTransform] = playback;
        }

        public static Transform ResolveVisualTransform(Transform root)
        {
            Animator animator = root.GetComponentInChildren<Animator>();

            if (animator != null)
                return animator.transform;

            return root;
        }

        private static void StopPlayback(Transform visualTransform)
        {
            if (ActivePlaybacks.TryGetValue(visualTransform, out Playback activePlayback) == false)
                return;

            activePlayback.PositionTween?.Kill();
            activePlayback.ScaleTween?.Kill();
            visualTransform.localScale = activePlayback.BaseLocalScale;
            ActivePlaybacks.Remove(visualTransform);
        }

        private static void FinishPlayback(Transform visualTransform, Playback playback)
        {
            if (ActivePlaybacks.TryGetValue(visualTransform, out Playback activePlayback) == false
                || activePlayback != playback)
            {
                return;
            }

            visualTransform.localScale = playback.BaseLocalScale;
            ActivePlaybacks.Remove(visualTransform);
        }

        private sealed class Playback
        {
            public Tween PositionTween;
            public Tween ScaleTween;
            public Vector3 BaseLocalScale;
        }
    }
}
