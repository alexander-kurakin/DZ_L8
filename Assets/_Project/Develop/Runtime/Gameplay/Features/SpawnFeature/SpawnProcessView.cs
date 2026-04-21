using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.SpawnFeature
{
    [RequireComponent(typeof(Animator))]
    public class SpawnProcessView : EntityView
    {
        private readonly int SpawningProcessKey = Animator.StringToHash("InSpawnProcess");

        [SerializeField] private Animator _animator;
        [SerializeField] private ParticleSystem _spawnEffectPrefab;
        [SerializeField] private Transform _spawnEffectPoint;
        [SerializeField] private ParticleSystem _spawnGlowPrefab;

        private ReactiveVariable<bool> _inSpawnProcess;
        private Transform _entityTransform;
        private ParticleSystem _spawnGlowInstance;

        private IDisposable _inSpawnProcessChangedDisposable;

        private void OnValidate()
        {
            _animator ??= GetComponent<Animator>();
        }

        protected override void OnEntityStartedWork(Entity entity)
        {
            _inSpawnProcess = entity.InSpawnProcess;
            _entityTransform =  entity.Transform;

            _inSpawnProcessChangedDisposable = _inSpawnProcess.Subscribe(OnSpawnProcessChanged);
            OnSpawnProcessChanged(default, _inSpawnProcess.Value);
        }

        public override void Cleanup(Entity entity)
        {
            base.Cleanup(entity);

            _inSpawnProcessChangedDisposable.Dispose();
        }

        private void OnSpawnProcessChanged(bool arg1, bool newValue)
        {
            UpdateSpawnProcessKey(newValue);
            
            if (newValue)
            {
                Instantiate(
                    _spawnEffectPrefab,
                    new Vector3(_spawnEffectPoint.position.x, _spawnEffectPoint.position.y + 2.5f, _spawnEffectPoint.position.z),
                    _spawnEffectPrefab.transform.rotation,
                    null);
                
                _spawnGlowInstance = Instantiate(
                    _spawnGlowPrefab,
                    new Vector3 (_entityTransform.position.x, _entityTransform.position.y + 2.5f, _entityTransform.position.z),
                    Quaternion.identity,
                    null);
            }
            else
            {
                if (_spawnGlowInstance == null)
                    return;
                
                foreach (ParticleSystem subSystem in _spawnGlowInstance.GetComponentsInChildren<ParticleSystem>())
                {
                    subSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                }
            }
        }

        private void UpdateSpawnProcessKey(bool value)
        {
            _animator.SetBool(SpawningProcessKey, value);
        }
    }
}
