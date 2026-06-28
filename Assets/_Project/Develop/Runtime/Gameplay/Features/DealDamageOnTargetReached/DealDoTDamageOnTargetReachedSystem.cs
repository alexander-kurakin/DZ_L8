using System;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Gameplay.Features.PlantCombatFeature;
using Assets._Project.Develop.Runtime.Utilities.Reactive;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.DealDamageOnTargetReached
{
    public class DealDoTDamageOnTargetReachedSystem : IInitializableSystem, IDisposableSystem, IUpdatableSystem
    {
        private readonly DragonEnrageService _dragonEnrageService;

        private IDisposable _targetReachedRequest;
        private ReactiveVariable<Entity> _target;
        private Entity _source;
        
        private ReactiveVariable<float> _damagePerTick;
        private ReactiveVariable<float> _dotTickInterval;
        private ReactiveVariable<float> _dotTimer;
        
        private bool _targetReached;

        public DealDoTDamageOnTargetReachedSystem(DragonEnrageService dragonEnrageService)
        {
            _dragonEnrageService = dragonEnrageService;
        }
        
        public void OnInit(Entity entity)
        {
            _target = entity.CurrentTarget;
            _source = entity;
            
            _damagePerTick = entity.DamagePerTick;
            _dotTickInterval = entity.DamageInterval;
            _dotTimer = entity.DamageTimer;

            _targetReachedRequest = entity.DistanceToTargetReachedEvent.Subscribe(OnTargetReached);
        }

        private void OnTargetReached()
        {
            if (_target.Value == null)
                return;

            _targetReached = true;
        }
        
        public void OnDispose()
        {
            _targetReachedRequest.Dispose();
        }

        public void OnUpdate(float deltaTime)
        {
            if (_targetReached == false)
                return;

            _dotTimer.Value -= deltaTime;

            if (_dotTimer.Value > 0f)
                return;

            float outgoingDamageMultiplier = _dragonEnrageService.GetOutgoingDamageMultiplier(_source);
            float damage = _damagePerTick.Value * outgoingDamageMultiplier;

            EntitiesHelper.TryTakeDamageFrom(_source, _target.Value, damage);
            
            _dotTimer.Value = _dotTickInterval.Value;
        }
    }
}
