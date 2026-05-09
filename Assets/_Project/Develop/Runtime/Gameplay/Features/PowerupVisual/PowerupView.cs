using System;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using DamageNumbersPro;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.PowerupVisual
{
    public class PowerupView : EntityView
    {
        [SerializeField] private Transform _effectSpawnPoint;
        [SerializeField] private DamageNumber _healNumberPrefab;
        [SerializeField] private DamageNumber _damageNumberPrefab;

        private IDisposable _healthChangedDisposable;
        
        protected override void OnEntityStartedWork(Entity entity)
        {
            _healthChangedDisposable = entity.CurrentHealth.Subscribe(OnHealthChanged);
            
            _healNumberPrefab.SetScale(3f);
            _healNumberPrefab.SetColor(Color.green);
            
            _damageNumberPrefab.SetScale(7f);
        }

        private void OnHealthChanged(float oldValue, float newValue)
        {
            float absNumberChange = MathF.Abs(oldValue - newValue);
            
            if (oldValue > newValue)
                _damageNumberPrefab.Spawn(_effectSpawnPoint.position, absNumberChange);  
            else
                _healNumberPrefab.Spawn(_effectSpawnPoint.position, absNumberChange);
        }

        public override void Cleanup(Entity entity)
        {
            base.Cleanup(entity);
            
            _healthChangedDisposable?.Dispose();
        }
    }
}