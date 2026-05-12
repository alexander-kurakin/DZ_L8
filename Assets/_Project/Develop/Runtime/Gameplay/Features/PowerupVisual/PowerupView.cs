using System;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using Assets._Project.Develop.Runtime.Utilities.Audio;
using DamageNumbersPro;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.PowerupVisual
{
    public class PowerupView : EntityView
    {
        [SerializeField] private Transform _effectSpawnPoint;
        [SerializeField] private DamageNumber _healNumberPrefab;
        [SerializeField] private DamageNumber _damageNumberPrefab;
        
        [SerializeField] private GameSoundsIDs _healSoundToPlay;
        [SerializeField] private GameSoundsIDs _damagedSoundToPlay;
        [SerializeField] private AudioSource _localAudioSource;

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
            {
                _damageNumberPrefab.Spawn(_effectSpawnPoint.position, absNumberChange);
                GameSoundsService.PlayOneShot(_damagedSoundToPlay, _localAudioSource);
            }
            else
            {
                _healNumberPrefab.Spawn(_effectSpawnPoint.position, absNumberChange);
                GameSoundsService.PlayOneShot(_healSoundToPlay, _localAudioSource);
            }
        }

        public override void Cleanup(Entity entity)
        {
            base.Cleanup(entity);
            
            _healthChangedDisposable?.Dispose();
        }
    }
}