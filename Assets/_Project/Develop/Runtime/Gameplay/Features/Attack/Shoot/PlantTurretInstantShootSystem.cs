using Assets._Project.Develop.Runtime.Configs.Gameplay.Stages;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Gameplay.Features.PlantCombatFeature;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Attack.Shoot
{
    public class PlantTurretInstantShootSystem : IInitializableSystem, IDisposableSystem
    {
        private readonly EntitiesFactory _entitiesFactory;
        private readonly PlantDamageCounterService _plantDamageCounterService;

        private ReactiveEvent _attackDelayEndEvent;
        private Entity _entity;
        private ReactiveVariable<float> _damage;
        private Transform _shootPoint;
        private IDisposable _attackDelayEndDisposable;

        public PlantTurretInstantShootSystem(
            EntitiesFactory entitiesFactory,
            PlantDamageCounterService plantDamageCounterService)
        {
            _entitiesFactory = entitiesFactory;
            _plantDamageCounterService = plantDamageCounterService;
        }

        public void OnInit(Entity entity)
        {
            _entity = entity;
            _attackDelayEndEvent = entity.AttackDelayEndEvent;
            _damage = entity.InstantAttackDamage;
            _shootPoint = entity.ShootPoint;
            _attackDelayEndDisposable = _attackDelayEndEvent.Subscribe(OnAttackDelayEnd);
        }

        private void OnAttackDelayEnd()
        {
            float shotDamage = ResolveShotDamage();
            _entitiesFactory.CreateProjectile(_shootPoint.position, _shootPoint.forward, shotDamage, _entity);
        }

        private float ResolveShotDamage()
        {
            Entity target = _entity.CurrentTarget.Value;

            if (target == null)
                return _damage.Value;

            if (target.TryGetEnemyWavePreviewType(out WaveEnemyPreviewType previewType) == false)
                return _damage.Value;

            float damageMultiplier = _plantDamageCounterService.GetDamageMultiplier(PlantDamageSource.Turret, previewType);
            return _damage.Value * damageMultiplier;
        }

        public void OnDispose()
        {
            _attackDelayEndDisposable.Dispose();
        }
    }
}
