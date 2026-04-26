using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Utilities;
using Assets._Project.Develop.Runtime.Utilities.Reactive;

namespace _Project.Develop.Runtime.Gameplay.Features.DealDamageOverTime
{
    public class DamageOverTimeSystem : IInitializableSystem, IUpdatableSystem
    {
        private Entity _damageSource;
        private Buffer<Entity> _contactEntities;
        
        private ReactiveVariable<float> _damagePerTick;
        private ReactiveVariable<float> _dotTickInterval;
        private ReactiveVariable<float> _dotTimer;
        
        public void OnInit(Entity entity)
        {
            _damageSource = entity;
            _contactEntities = entity.ContactEntitiesBuffer;
            
            _damagePerTick = entity.DamagePerTick;
            _dotTickInterval = entity.DamageInterval;
            _dotTimer = entity.DamageTimer;
        }

        public void OnUpdate(float deltaTime)
        {
            _dotTimer.Value -= deltaTime;

            if (_dotTimer.Value > 0f)
                return;

            for (int i = 0; i < _contactEntities.Count; i++)
                EntitiesHelper.TryTakeDamageFrom(_damageSource, _contactEntities.Items[i], _damagePerTick.Value);
            
            _dotTimer.Value = _dotTickInterval.Value;
        }
    }
}