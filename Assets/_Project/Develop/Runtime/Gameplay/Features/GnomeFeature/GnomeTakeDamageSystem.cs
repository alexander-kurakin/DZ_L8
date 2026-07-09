using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Gameplay.Features.RunKillCounter;
using Assets._Project.Develop.Runtime.Gameplay.Features.TakeDamage;
using Assets._Project.Develop.Runtime.Utilities.Conditions;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.GnomeFeature
{
    public class GnomeTakeDamageSystem : IInitializableSystem, IDisposableSystem
    {
        private ReactiveEvent<TakeDamageInfo> _damageRequest;
        private ReactiveEvent<TakeDamageInfo> _damageEvent;
        private ICompositeCondition _canTakeDamage;
        private ReactiveVariable<int> _hitCount;
        private int _requiredHits;
        private ReactiveVariable<bool> _isDead;

        private IDisposable _damageRequestSubscription;

        public void OnInit(Entity entity)
        {
            _damageRequest = entity.TakeDamageRequest;
            _damageEvent = entity.TakeDamageEvent;
            _canTakeDamage = entity.CanTakeDamage;
            _hitCount = entity.GnomeHitCount;
            _requiredHits = entity.GnomeRequiredHits;

            if (entity.TryGetComponent(out RunEnemyKillMarker killMarker) == false)
                throw new InvalidOperationException("Gnome entity is missing RunEnemyKillMarker.");

            _isDead = killMarker.IsDead;
            _damageRequestSubscription = _damageRequest.Subscribe(OnDamageRequest);
        }

        public void OnDispose()
        {
            _damageRequestSubscription?.Dispose();
        }

        private void OnDamageRequest(TakeDamageInfo damageInfo)
        {
            if (_isDead.Value == true)
                return;

            if (_canTakeDamage.Evaluate() == false)
                return;

            _hitCount.Value++;
            _damageEvent.Invoke(new TakeDamageInfo(1f, damageInfo.Source));

            if (_hitCount.Value < _requiredHits)
                return;

            _isDead.Value = true;
        }
    }
}
