using System;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using Assets._Project.Develop.Runtime.Gameplay.Features.JuiceFeature;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Mines
{
    public class MineDetonationView : EntityView
    {
        [SerializeField] private GameObject _detonationEffectPrefab;
        [SerializeField] private float _detonationEffectScale = 10f;
        [SerializeField] private Vector3 _localScaleMultiplier = new Vector3(2f, 1f, 2f);

        private IDisposable _detonationDisposable;

        protected override void OnEntityStartedWork(Entity entity)
        {
            _detonationDisposable = entity.DealAreaImpactDamageRequest.Subscribe(OnDetonationRequested);
        }

        private void OnDetonationRequested(Vector3 worldPoint)
        {
            if (_detonationEffectPrefab == null)
                return;

            GameObject instance = GameplayVfxUtility.SpawnAt(
                _detonationEffectPrefab,
                worldPoint,
                Quaternion.identity,
                null,
                1f);

            if (instance == null)
                return;

            instance.transform.localScale = new Vector3(
                _detonationEffectScale * _localScaleMultiplier.x,
                _detonationEffectScale * _localScaleMultiplier.y,
                _detonationEffectScale * _localScaleMultiplier.z);
            GameplayVfxUtility.ScheduleDestroyAfterLifetime(instance);
        }

        public override void Cleanup(Entity entity)
        {
            base.Cleanup(entity);

            _detonationDisposable?.Dispose();
        }
    }
}
