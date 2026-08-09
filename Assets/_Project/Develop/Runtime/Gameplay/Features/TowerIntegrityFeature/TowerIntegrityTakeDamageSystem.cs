using System;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Gameplay.Features.Ability;
using Assets._Project.Develop.Runtime.Gameplay.Features.TakeDamage;
using Assets._Project.Develop.Runtime.Utilities.Conditions;
using Assets._Project.Develop.Runtime.Utilities.Reactive;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.TowerIntegrityFeature
{
    public class TowerIntegrityTakeDamageSystem : IInitializableSystem, IDisposableSystem
    {
        private readonly TowerIntegrityLeakResolver _leakResolver;
        private readonly SpellcoreCoachToastService _spellcoreCoachToastService;

        private ReactiveEvent<TakeDamageInfo> _damageRequest;
        private ReactiveEvent<TakeDamageInfo> _damageEvent;
        private ReactiveVariable<float> _health;
        private ICompositeCondition _canTakeDamage;
        private IDisposable _requestDisposable;

        public TowerIntegrityTakeDamageSystem(
            TowerIntegrityLeakResolver leakResolver,
            SpellcoreCoachToastService spellcoreCoachToastService)
        {
            _leakResolver = leakResolver;
            _spellcoreCoachToastService = spellcoreCoachToastService;
        }

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
            if (_canTakeDamage.Evaluate() == false)
                return;

            int hits = _leakResolver.ResolveHits(damageInfo.Source);

            if (hits <= 0)
                return;

            _health.Value = MathF.Max(_health.Value - hits, 0);

            TakeDamageInfo integrityDamageInfo = new TakeDamageInfo(hits, damageInfo.VisualKind, damageInfo.Source);
            _damageEvent.Invoke(integrityDamageInfo);

            _spellcoreCoachToastService.TryOnTowerIntegrityDamaged(hits);
        }
    }
}
