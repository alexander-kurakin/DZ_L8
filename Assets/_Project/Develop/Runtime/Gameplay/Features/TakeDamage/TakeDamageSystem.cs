using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Utilities.Conditions;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.TakeDamage
{
    public class TakeDamageSystem : IInitializableSystem, IDisposableSystem
    {
        private ReactiveEvent<TakeDamageInfo> _damageRequest;
        private ReactiveEvent<TakeDamageInfo> _damageEvent;

        private ReactiveVariable<float> _health;

        private ICompositeCondition _canTakeDamage;

        private IDisposable _requestDisposable;

        public void OnInit(Entity entity)
        {
            _damageRequest = entity.TakeDamageRequest;
            _damageEvent = entity.TakeDamageEvent;

            _health = entity.CurrentHealth;

            _canTakeDamage = entity.CanTakeDamage;

            _requestDisposable = _damageRequest.Subscribe(OnDamageRequest);
        }

        public void OnDispose()
        {
            _requestDisposable.Dispose();
        }

        private void OnDamageRequest(TakeDamageInfo damageInfo)
        {
            if (damageInfo.Damage < 0)
                throw new ArgumentOutOfRangeException(nameof(damageInfo));

            if (_canTakeDamage.Evaluate() == false)
                return;

            _health.Value = MathF.Max(_health.Value - damageInfo.Damage, 0);
            _damageEvent.Invoke(damageInfo);
        }
    }
}
