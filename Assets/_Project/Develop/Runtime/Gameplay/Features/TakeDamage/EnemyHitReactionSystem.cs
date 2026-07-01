using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.TakeDamage
{
    public class EnemyHitReactionSystem : IInitializableSystem, IUpdatableSystem, IDisposableSystem
    {
        private const float REFERENCE_DAMAGE = 50f;
        private const float MIN_HIT_STUN_DURATION_SECONDS = 0.12f;
        private const float MAX_HIT_STUN_DURATION_SECONDS = 0.22f;

        private ReactiveEvent<TakeDamageInfo> _damageEvent;
        private ReactiveVariable<float> _hitStunRemainingTime;
        private Rigidbody _rigidbody;
        private IDisposable _damageSubscription;

        public void OnInit(Entity entity)
        {
            _damageEvent = entity.TakeDamageEvent;
            _hitStunRemainingTime = entity.EnemyHitStunRemainingTime;
            _rigidbody = entity.Rigidbody;
            _damageSubscription = _damageEvent.Subscribe(OnDamageTaken);
        }

        public void OnUpdate(float deltaTime)
        {
            if (_hitStunRemainingTime.Value <= 0f)
                return;

            _hitStunRemainingTime.Value = Mathf.Max(0f, _hitStunRemainingTime.Value - deltaTime);
        }

        public void OnDispose()
        {
            _damageSubscription.Dispose();
        }

        private void OnDamageTaken(TakeDamageInfo damageInfo)
        {
            if (damageInfo.VisualKind == TakeDamageVisualKind.Toxic)
                return;

            float damageFactor = Mathf.Clamp01(damageInfo.Damage / REFERENCE_DAMAGE);
            float hitStunDuration = Mathf.Lerp(MIN_HIT_STUN_DURATION_SECONDS, MAX_HIT_STUN_DURATION_SECONDS, damageFactor);
            _hitStunRemainingTime.Value = Mathf.Max(_hitStunRemainingTime.Value, hitStunDuration);

            _rigidbody.velocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
        }
    }
}
